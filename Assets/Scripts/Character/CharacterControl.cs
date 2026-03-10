using System.Collections.Generic;
using Buff;
using Character.Interfaces;
using Character.StateMachine;
using Character.StateMachine.States;
using Commons;
using Databrain;
using Items.ItemData;
using Level;
using Manager;
using Monster;
using Skill.SkillData;
using StatusEffect;
using UnityEngine;
using UnityEngine.Events;
using PlayerInput = Player.PlayerInput;

namespace Character
{
    /// <summary>
    /// キャラクターの中心となるコントローラー
    /// 各サブコンポーネント（Movement, Combat, StatusUI, Inventory）を統合・調整する
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterMovement))]
    [RequireComponent(typeof(CharacterCombat))]
    [RequireComponent(typeof(CharacterStatusUI))]
    [RequireComponent(typeof(CharacterInventory))]
    [RequireComponent(typeof(StatusEffectManager))]
    public class CharacterControl : MonoBehaviour, ICharacterStats
    {
        #region Serialized Fields

        [Header("Character Settings")]
        [SerializeField] private int _characterId;
        [SerializeField] private string _characterCategory;
        [SerializeField] private DataLibrary _dataLibrary;
        [SerializeField] private bool _isFacingRight = true;

        [Header("References")]
        [SerializeField] private GameObject _meshCharacter;
        [SerializeField] private InputBase _input;

        [Header("Events")]
        [SerializeField] private UnityEvent _onJump;
        [SerializeField] private UnityEvent _onLand;
        [SerializeField] private UnityEvent _onFast;
        [SerializeField] private UnityEvent _onSprint;
        [SerializeField] private UnityEvent _onCrouch;

        [Header("Event Thresholds")]
        [SerializeField] private float _minimumVerticalSpeedToLandEvent;
        [SerializeField] private float _minimumHorizontalSpeedToFastEvent;

        #endregion

        #region Private Fields - Components

        private CharacterMovement _movement;
        private CharacterCombat _combat;
        private CharacterStatusUI _statusUI;
        private CharacterInventory _inventory;

        private Rigidbody _rigidbody;
        private Animator _animator;
        private PlayerInput _playerInput;
        private Canvas _userUI;
        private CharacterStateMachine _stateMachine;

        #endregion

        #region Private Fields - Status

        private MetaStatus _metaStatus;
        private BehaviourStatus _behaviourStatus;
        private AttributeStatus _attributeStatus;
        private BattleStatus _battleStatus;

        private int _maxHealth;
        private int _maxStamina;
        private int _maxGuardHealth;
        private List<float> _extraHealths;

        #endregion

        #region Private Fields - Buff System

        private IBuffManager _buffManager;

        #endregion

        #region Private Fields - Status Effect System

        private StatusEffectManager _statusEffectManager;

        // StatusEffectによる行動制限フラグ（StateMachineの状態とは独立）
        // ICharacterStats.CanXxx.set から更新される
        private bool _statusCanMove = true;
        private bool _statusCanAttack = true;
        private bool _statusCanGuard = true;

        #endregion

        #region Private Fields - Knockback

        private float _knockbackTimer;

        #endregion

        #region Private Fields - Rolling

        private float _rollingCooldownTimer;

        #endregion

        #region Private Fields - Input State

        private Vector2 _axisInput;
        private bool _jump;
        private bool _sprint;
        private bool _attack;
        private bool _guard;
        private bool _modeChange;
        private bool _roll;
        private float _rollingDirection;

        #endregion

        #region Private Fields - State

        private bool _isDead;
        private bool _isOnUI;
        private int _uniqueId;
        private float _prevRotate = float.NaN;
        private float _killerLuck;
        private GameObject _target;

        private int _currentLevel;
        private int _abilityPoints;
        private AbilityTreeManager _abilityTreeManager;
        private float _attackHeldTimer;
        private int _chargeLevel;

        #endregion

        #region Public Properties

        public int CharacterId => _characterId;
        public DataLibrary DataLibrary => _dataLibrary;
        public InputBase Input => _input;
        public GameObject MeshCharacter => _meshCharacter;
        public MetaStatus MetaStatus => _metaStatus;
        public BattleStatus BattleStatus => _battleStatus;
        public float PrevRotate { get => _prevRotate; set => _prevRotate = value; }
        public bool IsKnockedBack => _knockbackTimer > 0;
        public float RotateY { get => _movement.RotateY; }

        public CharacterStateMachine StateMachine => _stateMachine;

        // Inventory shortcuts
        public CharacterSlotItemData CharacterItems => _inventory.CharacterItems;
        public bool[] isAttackItems => _inventory.IsAttackItems;
        public bool[] isNullItems => _inventory.IsNullItems;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            InitializeStatus();
            InitializeBuffSystem();
            InitializeSubComponents();
            InitializeStateMachine();
            InitializeAbilityTree();
        }

        private void OnDestroy()
        {
            DamageManager.RemoveCharacter(this);
        }

        private void Start()
        {
            DamageManager.AddRequest(this);
            _inventory.SetupItemIcons();
            _statusUI.UpdateAttribute(GetAttributeArray());
        }

        private void Update()
        {
            ReadInput();
            CheckOnUI();

            // UI updates
            _statusUI.UpdateUI(GetHealth(), GetStamina());
            _statusUI.UpdateAttribute(GetAttributeArray());
            FocusTarget();

            // Cooldowns
            _inventory.DecreaseFireCooldowns(Time.deltaTime);
            _combat.DecreaseGuardCooldowns(Time.deltaTime);
            _movement.DecreaseJumpCooldown(Time.deltaTime);
            _buffManager.Update(Time.deltaTime);
            _combat.UpdateComboTimer(Time.deltaTime);
            if (_rollingCooldownTimer > 0f) _rollingCooldownTimer -= Time.deltaTime;

            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _movement.FixZAxis();

            if (_isDead)
            {
                _movement.StopMovement();
            }
            else
            {
                // Physics checks and combat
                CheckDead();

                // StateMachine + StatusEffect の複合で行動可否を判定
                bool canMove = _stateMachine.CanMove && _statusCanMove;
                bool canGuard = _stateMachine.CanGuard && _statusCanGuard;

                bool canProcessGuard = canGuard && _guard;
                _combat.ProcessGuard(canProcessGuard);
                RegainStamina();

                // ノックバックタイマー減算
                if (_knockbackTimer > 0)
                    _knockbackTimer -= Time.fixedDeltaTime;

                Vector2 moveInput = (canMove && !IsKnockedBack) ? _axisInput : Vector2.zero;
                _movement.ProcessMovement(moveInput, canMove && !IsKnockedBack && _jump);

                // Events
                UpdateEvents();
            }

            _stateMachine.FixedUpdate();
            _movement.ProcessGravity();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            _movement = GetComponent<CharacterMovement>();
            _combat = GetComponent<CharacterCombat>();
            _statusUI = GetComponent<CharacterStatusUI>();
            _inventory = GetComponent<CharacterInventory>();
            _statusEffectManager = GetComponent<StatusEffectManager>();

            _rigidbody = GetComponent<Rigidbody>();
            _animator = _meshCharacter.GetComponent<Animator>();
            _target = _meshCharacter.transform.Find("Target")?.gameObject;

            if (_input is PlayerInput playerInput)
            {
                _playerInput = playerInput;
                _userUI = GameObject.Find("UserUI")?.GetComponent<Canvas>();
            }
        }

        private void InitializeStatus()
        {
            StatusLibrary statusData = new StatusLibrary(_dataLibrary);
            _metaStatus = statusData.SelectMeta(_characterId);
            _behaviourStatus = _metaStatus.behaviour;
            _attributeStatus = _metaStatus.attribute;

#if UNITY_EDITOR
            _battleStatus = ScriptableObject.CreateInstance<BattleStatus>().Clone(_metaStatus.battle);
#else
            _battleStatus = _metaStatus.battle;
#endif

            _maxHealth = (int)_battleStatus.health;
            _maxStamina = (int)_battleStatus.stamina;
            _maxGuardHealth = (int)_battleStatus.guardHealth;
            _extraHealths = new List<float>();

            _currentLevel = _metaStatus.level;
            _abilityPoints = 0;
        }

        private void InitializeBuffSystem()
        {
            _buffManager = new BuffManager();
        }

        private void InitializeSubComponents()
        {
            bool isPlayer = _playerInput != null;
            Canvas hpBarCanvas = isPlayer
                ? _userUI?.transform.Find("PlayerStatus")?.GetComponent<Canvas>()
                : gameObject.transform.Find("HPBar")?.GetComponent<Canvas>();

            // Movement初期化
            _movement.Initialize(this, _animator);

            // Combat初期化
            _combat.Initialize(this, _animator, _playerInput, _battleStatus.guardNum, _maxGuardHealth);

            // StatusUI初期化
            _statusUI.Initialize(this, _combat, _userUI, hpBarCanvas,
                _maxHealth, _maxStamina, _battleStatus.guardNum, _metaStatus.level, isPlayer);

            // Inventory初期化
            _inventory.Initialize(this, _statusUI, _dataLibrary, _metaStatus.id, _metaStatus.level);

            // StatusEffectManager初期化
            _statusEffectManager.Initialize(this);
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new CharacterStateMachine(this, _movement, _combat, _animator);
            _stateMachine.ForceChangeState<IdleState>();
        }

        private void InitializeAbilityTree()
        {
            _abilityTreeManager = new AbilityTreeManager(this, _characterId, _characterCategory);
        }

        #endregion

        #region Input

        private void ReadInput()
        {
            _axisInput = _input.move;
            _modeChange = _input.modeChange;
            _jump = _input.jump;
            _guard = _input.guard;
            _attack = _input.attack && !_guard;

            // チャージタイマー: 攻撃ボタン長押し中に加算（ガード中はリセット）
            if (_attack)
                _attackHeldTimer += Time.deltaTime;
            else
                _attackHeldTimer = 0f;

            if (_input.roll && _rollingCooldownTimer <= 0f)
            {
                _roll = true;
                _rollingDirection = _input.rollDirection;
            }
            else
            {
                _roll = false;
            }
        }

        private void CheckOnUI()
        {
            if (!_playerInput || !_attack)
            {
                _isOnUI = false;
                return;
            }

            bool onUI = false;
            Vector3 clickPos = _playerInput.WorldMousePoint(GameBalance.MOUSE_POINT_Z_DISTANCE);

            foreach (Transform ui in _userUI.transform)
            {
                if (ui.name == "Aim") continue;

                Vector3[] corners = new Vector3[4];
                ui.GetComponent<RectTransform>().GetWorldCorners(corners);

                float minX = corners[0].x, minY = corners[0].y;
                float maxX = corners[2].x, maxY = corners[2].y;

                onUI = clickPos.x > minX && clickPos.x < maxX && clickPos.y > minY && clickPos.y < maxY;
                if (onUI) break;
            }
            _isOnUI = onUI;
        }

        #endregion

        #region Status Management

        private void RegainStamina()
        {
            if (Mathf.Approximately(_battleStatus.stamina, _maxStamina)) return;

            if (_battleStatus.stamina > _maxStamina)
            {
                _battleStatus.stamina = _maxStamina;
                return;
            }

            // 不安状態の場合はスタミナ回復をスキップ
            if (_statusEffectManager != null && _statusEffectManager.IsStaminaRegenBlocked)
                return;

            _battleStatus.stamina += Time.fixedDeltaTime;
        }

        private void CheckDead()
        {
            if (_battleStatus.health > 0) return;

            _isDead = true;
            _combat.CheckDead(_battleStatus.health);
            _stateMachine.ForceChangeState<DeadState>();
        }

        #endregion

        #region Public Methods - Attack

        public void AttackMotion()
        {
            _combat.AttackMotion(_isOnUI, _attack);
        }

        public int GetComboCount() => _combat.ComboCount;
        public float GetComboDamageMultiplier() => _combat.GetComboDamageMultiplier();

        #endregion

        #region Public Methods - Damage/Heal (DamageManagerから呼び出し)

        public void DamageInflicted(float damage, Vector3 effectPosition, float killerLuck, float knockbackPower = 0f)
        {
            _combat.DamageInflicted(damage, effectPosition, killerLuck, knockbackPower,
                ref _battleStatus.health, ref _extraHealths, luck => _killerLuck = luck);
            _statusUI.UpdateExtraHealth(_extraHealths);

            if (!_isDead)
                _stateMachine.ForceChangeState<DamagedState>();
        }

        public void HealInflicted(float heal, bool overHeal, Vector3 effectPosition)
        {
            _combat.HealInflicted(heal, overHeal, effectPosition,
                ref _battleStatus.health, _maxHealth, ref _extraHealths);
            _statusUI.UpdateExtraHealth(_extraHealths);
        }

        public float GuardInflicted(float damage, float cooldown)
        {
            return _combat.GuardInflicted(damage, cooldown);
        }

        /// <summary>
        /// ノックバックを適用する
        /// </summary>
        public void ApplyKnockback(Vector3 direction, float power)
        {
            _movement.ApplyKnockback(direction, power);
            _knockbackTimer = GameBalance.KNOCKBACK_INPUT_LOCK_DURATION;
        }

        public void DecreaseShield(float variable)
        {
            _inventory.DecreaseShield(variable, _buffManager.GetBuffValue(BuffType.Shield));
        }

        #endregion

        #region Public Methods - Skill/Item (Inventoryの委譲)

        public bool IncreaseFireCooldown(int num, float addTime) => _inventory.SetFireCooldown(num, addTime);
        public float GetFireCooldown(int num) => _inventory.GetFireCooldown(num);

        public bool UseSkill(SkillData skill)
        {
            return _inventory.UseSkill(skill, GetThickness(), GetStrength(), ref _battleStatus.stamina);
        }

        public bool SwapSkillActive(int number) => _inventory.SwapSkillActive(number);
        public bool GetIsSkillActivate(int number) => _inventory.GetIsSkillActivate(number);
        public bool[] GetActiveAttackSkills() => _inventory.GetActiveAttackSkills();
        public ExpendableItemData GetSlotItem(int slot) => _inventory.GetSlotItem(slot);
        public SkillData GetCacheSkill(int slot) => _inventory.GetCacheSkill(slot);
        public SkillData GetSkillByGuid(string guid) => _inventory.GetSkillByGuid(guid);
        public OwnedItemData GetItemByGuid(string guid) => _inventory.GetItemByGuid(guid);

        public (bool result, int remain) AddItem(OwnedItemData itemData, int amount)
        {
            return _inventory.AddItem(itemData, amount);
        }

        public void SaveItem() => _inventory.SaveItem();

        #endregion

        #region Public Methods - State Getters

        public Vector3 GetPosition() => _movement.GetPosition();
        public float GetHeight() => _movement.GetHeight();
        public bool GetGrounded() => _movement.IsGrounded;
        public bool GetTouchingSlope() => _movement.IsTouchingSlope;
        public bool GetTouchingStep() => _movement.IsTouchingStep;
        public bool GetTouchingWall() => _movement.IsTouchingWall;
        public bool GetJumping() => _movement.IsJumping;
        public bool GetCrouching() => _combat.IsCrouching;
        public bool GetAttack() => _attack;
        public bool GetGuard() => _guard;
        public bool GetIsDead() => _isDead;
        public bool GetIsOnUI() => _isOnUI;
        public bool GetModeChange() => _modeChange;
        public float GetOriginalColliderHeight() => _movement.OriginalColliderHeight * GameBalance.COLLIDER_HEIGHT_DISPLAY_MULTIPLIER;
        public int GetUniqueId() => _uniqueId;
        public float GetRigidPositionDiffX(Vector3 position) => _movement.GetRigidPositionDiffX(position);
        public float GetMinGuardCooldown() => _combat.GetMinGuardCooldown();

        public bool GetRoll() => _roll;
        public float GetRollingDirection() => _rollingDirection;
        public void StartRollingCooldown() => _rollingCooldownTimer = GameBalance.ROLLING_COOLDOWN;

        /// <summary>ガード入力とは独立した生の攻撃入力（パリィ反撃チェック用）</summary>
        public bool GetAttackInput() => _input.attack;

        /// <summary>パリィ成功時のスタミナ回復</summary>
        public void RecoverStaminaOnParry()
        {
            _battleStatus.stamina = Mathf.Min(
                _battleStatus.stamina + GameBalance.PARRY_STAMINA_RECOVERY,
                _maxStamina);
        }

        public int GetCurrentLevel() => _currentLevel;
        public int GetAbilityPoints() => _abilityPoints;
        public void AddAbilityPoints(int amount) { if (amount > 0) _abilityPoints += amount; }

        /// <summary>APを消費する。残高不足の場合は false を返し消費しない。</summary>
        public bool TrySpendAbilityPoints(int amount)
        {
            if (amount <= 0 || _abilityPoints < amount) return false;
            _abilityPoints -= amount;
            return true;
        }

        public int GetMaxHealth() => _maxHealth;

        /// <summary>
        /// 死亡時のオーバーキル比率（超過ダメージ / 最大HP）。生存中は 0 を返す。
        /// CharacterDropHandler がドロップ率補正に使用する。
        /// </summary>
        public float GetOverkillRatio()
        {
            if (!_isDead || _maxHealth <= 0) return 0f;
            float overkill = Mathf.Max(0f, -_battleStatus.health);
            return overkill / _maxHealth;
        }

        public AbilityTreeManager GetAbilityTreeManager() => _abilityTreeManager;

        /// <summary>スキルがアビリティツリーで解放済みかを返す。</summary>
        public bool IsAbilitySkillUnlocked(string skillGuid)
            => _abilityTreeManager?.IsSkillUnlocked(skillGuid) ?? false;

        /// <summary>チャージ攻撃に移行できる状態か（長押し時間がしきい値を超えているか）。</summary>
        public bool GetChargeReady() => _attackHeldTimer >= GameBalance.CHARGE_START_THRESHOLD;

        /// <summary>チャージ段階を記録する（ChargeState → ChargeAttackState 遷移時に呼び出す）。</summary>
        public void SetChargeLevel(int level) => _chargeLevel = level;

        /// <summary>記録されたチャージ段階を取得してリセットする（ChargeAttackState.OnEnterで使用）。</summary>
        public int ConsumeChargeLevel() { var l = _chargeLevel; _chargeLevel = 0; return l; }

        /// <summary>チャージ中に歩行可能か（アビリティツリー解放状態を参照）。</summary>
        public bool IsChargeWalkUnlocked() => IsAbilitySkillUnlocked("charge_walk");

        /// <summary>チャージ攻撃アニメーションをトリガーし、ダメージ倍率を設定する。</summary>
        public void ChargeAttackMotion() => _combat.ChargeAttackMotion(_chargeLevel);

        /// <summary>チャージダメージ倍率を返し 1.0 にリセットする（DamageManagerから呼び出し）。</summary>
        public float GetChargeDamageMultiplier() => _combat.GetChargeDamageMultiplier();

        /// <summary>
        /// レベルアップを適用する。_currentLevel をインクリメントし AP を付与する。
        /// NOTE: _metaStatus.level は起動時の初期値参照用。ランタイムのレベルは _currentLevel を参照。
        /// </summary>
        public void ApplyLevelUp(int apGained)
        {
            _currentLevel++;
            AddAbilityPoints(apGained);
        }

        #endregion

        #region Public Methods - Behaviour Stats (バフ込み)

        public float GetJumpTime() => _behaviourStatus.jumpTime + _buffManager.GetBuffValue(BuffType.JumpTime);
        public float GetMoveSpeed() => _behaviourStatus.moveSpeed + _buffManager.GetBuffValue(BuffType.MoveSpeed);
        public float GetJumpForce() => _behaviourStatus.jumpForce + _buffManager.GetBuffValue(BuffType.JumpForce);
        public float GetJumpSpeed() => _behaviourStatus.jumpSpeed + _buffManager.GetBuffValue(BuffType.JumpSpeed);
        public float GetCorePower() => _behaviourStatus.corePower + _buffManager.GetBuffValue(BuffType.CorePower);
        public float GetStrength() => _behaviourStatus.strength + _buffManager.GetBuffValue(BuffType.Strength);
        public float GetControl() => _behaviourStatus.control + _buffManager.GetBuffValue(BuffType.Control);
        public float GetThickness() => _behaviourStatus.thickness + _buffManager.GetBuffValue(BuffType.Thickness);
        public float GetEndurance() => _behaviourStatus.endurance + _buffManager.GetBuffValue(BuffType.Endurance);
        public float GetAdditionalGravity() => _behaviourStatus.additionalGravity + _buffManager.GetBuffValue(BuffType.AdditionalGravity);

        #endregion

        #region Public Methods - Battle Stats (バフ込み)

        public float GetShield() => _inventory.GetShieldValues() + _buffManager.GetBuffValue(BuffType.Shield);
        public float GetHealth() => _battleStatus.health + _buffManager.GetBuffValue(BuffType.Health);
        public float GetStamina() => _battleStatus.stamina + _buffManager.GetBuffValue(BuffType.Stamina);
        public float GetGuardHealth() => _battleStatus.guardHealth + _buffManager.GetBuffValue(BuffType.GuardHealth);
        public float GetRate() => _battleStatus.rate + _buffManager.GetBuffValue(BuffType.Rate);
        public float GetShieldAttackRate() => _battleStatus.shieldAttackRate + _buffManager.GetBuffValue(BuffType.ShieldAttackRate);
        public float GetAttackPower() => _battleStatus.attackPower + _buffManager.GetBuffValue(BuffType.AttackPower);
        public float GetDefencePower() => _battleStatus.defencePower + _buffManager.GetBuffValue(BuffType.DefensePower);
        public float GetBaseAttributePower() => _battleStatus.baseAttributePower + _buffManager.GetBuffValue(BuffType.BaseAttributePower);
        public float GetBaseResistancePower() => _battleStatus.baseResistancePower + _buffManager.GetBuffValue(BuffType.BaseResistancePower);
        public float GetMinRange() => _battleStatus.minRange + _buffManager.GetBuffValue(BuffType.MinRange);
        public float GetMaxRange() => _battleStatus.maxRange + _buffManager.GetBuffValue(BuffType.MaxRange);
        public float GetDiffusionRate() => _battleStatus.diffusionRate + _buffManager.GetBuffValue(BuffType.DiffusionRate);
        public float GetCriticalDamage() => _battleStatus.criticalDamage + _buffManager.GetBuffValue(BuffType.CriticalDamageRate);
        public float GetCriticalChance() => _battleStatus.criticalChance + _buffManager.GetBuffValue(BuffType.CriticalChance);
        public float GetGuardNum() => _battleStatus.guardNum + _buffManager.GetBuffValue(BuffType.GuardNum);

        #endregion

        #region Public Methods - Meta Stats

        public float GetLuck() => _metaStatus.luck + _buffManager.GetBuffValue(BuffType.Luck);
        public bool GetIsDrop() => _metaStatus.isDrop;
        public float GetDropRate() => _metaStatus.itemDropRate;
        public int GetDropNum() => _metaStatus.itemDropNum;
        public float GetKillerLuck() => _killerLuck;

        #endregion

        #region Public Methods - Other Stats

        public int GetAutoGuardNum() => (int)_buffManager.GetBuffValue(BuffType.AutoGuardNum);
        public float GetCooldownAccelerate() => _buffManager.GetBuffValue(BuffType.CooldownAccelerate);

        #endregion

        #region Public Methods - Attribute

        public float[] GetAttributeArray()
        {
            float[] attributeArray = new float[10];
            float[] baseArray = _attributeStatus.AsArray();
            float[] buffArray = {
                _buffManager.GetBuffValue(BuffType.Frame),
                _buffManager.GetBuffValue(BuffType.Aqua),
                _buffManager.GetBuffValue(BuffType.Plant),
                _buffManager.GetBuffValue(BuffType.Electric),
                _buffManager.GetBuffValue(BuffType.Ground),
                _buffManager.GetBuffValue(BuffType.Ice),
                _buffManager.GetBuffValue(BuffType.Oil),
                _buffManager.GetBuffValue(BuffType.Toxin),
                _buffManager.GetBuffValue(BuffType.Wind),
                _buffManager.GetBuffValue(BuffType.Spirit)
            };
            for (int i = 0; i < attributeArray.Length; i++)
            {
                attributeArray[i] = baseArray[i] + buffArray[i];
            }
            return attributeArray;
        }

        public (float value, AttributeMagnification.Attribute attribute) GetMaxAttribute()
        {
            var att = _attributeStatus.MaxAttribute();
            return (att.value + _battleStatus.baseAttributePower, att.attribute);
        }

        public float GetAttributeValue(AttributeMagnification.Attribute targetAttribute)
        {
            return targetAttribute is AttributeMagnification.Attribute.None
                ? 0
                : GetAttributeArray()[(int)targetAttribute] + _battleStatus.baseAttributePower;
        }

        #endregion

        #region Public Methods - Buff Management

        public void AddBuff(BuffType buffType, float buffValue, float buffTime) => _buffManager.Add(buffType, buffValue, buffTime);
        public void RemoveBuffByType(BuffType buffType) => _buffManager.RemoveBuffByType(buffType);
        public void RemoveDeBuffByType(BuffType buffType) => _buffManager.RemoveDeBuffByType(buffType);
        public void RemoveByType(BuffType buffType) => _buffManager.RemoveByType(buffType);
        public void RemoveAllBuff() => _buffManager.RemoveAll();
        public void DecTimeBuffByType(BuffType buffType, float time) => _buffManager.DecTimeBuffByType(buffType, time);
        public void IncTimeBuffByType(BuffType buffType, float time) => _buffManager.IncTimeBuffByType(buffType, time);
        public void DecValueBuffByType(BuffType buffType, float value) => _buffManager.DecValueBuffByType(buffType, value);

        #endregion

        #region Public Methods - Status Effect Management

        /// <summary>
        /// 状態異常を追加する
        /// </summary>
        public void AddStatusEffect(IStatusEffect effect) => _statusEffectManager.AddEffect(effect);

        /// <summary>
        /// 状態異常を削除する
        /// </summary>
        public void RemoveStatusEffect(string effectName) => _statusEffectManager.RemoveEffect(effectName);

        /// <summary>
        /// 指定した種類の状態異常を持っているか
        /// </summary>
        public bool HasStatusEffect(string effectName) => _statusEffectManager.HasEffect(effectName);

        /// <summary>
        /// 全ての状態異常を削除する
        /// </summary>
        public void ClearAllStatusEffects() => _statusEffectManager.ClearAllEffects();

        /// <summary>
        /// StatusEffectManagerへの参照を取得
        /// </summary>
        public StatusEffectManager StatusEffectManager => _statusEffectManager;

        #endregion

        #region ICharacterStats Implementation

        // Health
        float ICharacterStats.Health => GetHealth();
        float ICharacterStats.MaxHealth => _maxHealth;
        bool ICharacterStats.IsDead => _isDead;

        // Stamina
        float ICharacterStats.Stamina => GetStamina();
        float ICharacterStats.MaxStamina => _maxStamina;

        // Combat Stats
        float ICharacterStats.AttackPower => GetAttackPower();
        float ICharacterStats.DefensePower => GetDefencePower();
        float ICharacterStats.Shield => GetShield();
        float ICharacterStats.CriticalDamage => GetCriticalDamage();
        float ICharacterStats.CriticalChance => GetCriticalChance();
        float ICharacterStats.ShieldAttackRate => GetShieldAttackRate();
        float ICharacterStats.Luck => GetLuck();

        // State Flags — StateMachine（状態による制限）+ StatusEffect（状態異常による制限）の複合
        bool ICharacterStats.CanMove
        {
            get => _stateMachine.CanMove && _statusCanMove;
            set => _statusCanMove = value;
        }

        bool ICharacterStats.CanAttack
        {
            get => _stateMachine.CanAttack && _statusCanAttack;
            set => _statusCanAttack = value;
        }

        bool ICharacterStats.CanGuard
        {
            get => _stateMachine.CanGuard && _statusCanGuard;
            set => _statusCanGuard = value;
        }

        // Methods
        void ICharacterStats.TakeDamage(float damage)
        {
            DamageInflicted(damage, transform.position, 0f);
        }

        void ICharacterStats.Heal(float amount, bool allowOverheal)
        {
            HealInflicted(amount, allowOverheal, transform.position);
        }

        bool ICharacterStats.ConsumeStamina(float amount)
        {
            if (_battleStatus.stamina < amount) return false;
            _battleStatus.stamina -= amount;
            return true;
        }

        void ICharacterStats.RegenerateStamina(float deltaTime)
        {
            // 不安状態の場合はスタミナ回復しない
            if (_statusEffectManager.HasEffect("Anxiety")) return;
            RegainStamina();
        }

        void ICharacterStats.DecreaseShield(float amount)
        {
            DecreaseShield(amount);
        }

        #endregion

        #region Public Methods - Utility

        public void SetUniquePathAndHash()
        {
            _uniqueId = GameObjectUtility.GetUniqueId(gameObject);
        }

        #endregion

        #region Public Methods - Death Notification

        /// <summary>キャラクターが死亡したときに発火。PlayerCharacterController が購読する。</summary>
        public event System.Action OnCharacterDied;

        /// <summary>DeadState.OnEnter から呼ばれる。OnCharacterDied イベントを発火する。</summary>
        internal void NotifyDeath() => OnCharacterDied?.Invoke();

        /// <summary>
        /// 入力ソースを実行時に切り替える。プレイヤー操作 ↔ AI 操作の引き継ぎに使用。
        /// Awake 完了後（キャラクター初期化後）に呼ぶこと。
        /// </summary>
        public void SetInputBase(InputBase newInput)
        {
            _input = newInput;
            _playerInput = newInput as PlayerInput;
            _userUI = _playerInput != null
                ? GameObject.Find("UserUI")?.GetComponent<Canvas>()
                : null;
            _combat?.SetPlayerInput(_playerInput);
        }

        #endregion

        #region Private Methods - Events

        private void UpdateEvents()
        {
            if ((_jump && _movement.IsGrounded && _movement.CurrentSurfaceAngle <= _movement.MaxClimbableSlopeAngle) ||
                (_jump && !_movement.IsGrounded))
            {
                _onJump?.Invoke();
            }

            // Note: Land event needs velocity check from rigidbody
            if (_sprint) _onSprint?.Invoke();
            if (_combat.IsCrouching) _onCrouch?.Invoke();
        }

        private void FocusTarget()
        {
            if (_playerInput == null || _target == null) return;

            Vector3 cameraAngle = _playerInput.WorldMousePoint();
            Vector3 direction = new Vector3(cameraAngle.x, cameraAngle.y, 0f) - _target.transform.position;
            float directAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            float rotateY = _movement.RotateY;
            if (float.IsNaN(_prevRotate))
            {
                directAngle = rotateY == 0 ? -1f * directAngle : directAngle + 180f;
            }
            else
            {
                directAngle = _prevRotate == 0 ? -1f * directAngle : directAngle + 180f;
            }

            _target.transform.localRotation = Quaternion.Euler(directAngle, 90f, 0f);
        }

        #endregion
    }
}
