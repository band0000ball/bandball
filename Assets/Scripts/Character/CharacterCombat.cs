using System.Collections.Generic;
using Commons;
using DamageNumbersPro;
using static Commons.GameBalance;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Character
{
    /// <summary>
    /// キャラクターの戦闘処理を担当するコンポーネント
    /// 攻撃、ガード、ダメージ処理を管理
    /// </summary>
    [RequireComponent(typeof(CharacterControl))]
    public class CharacterCombat : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Hit Effects")]
        [SerializeField] private ParticleSystem _hitEffect;
        [SerializeField] private float _hitStopTime = GameBalance.HIT_STOP_DURATION;

        [Header("Damage Numbers")]
        [SerializeField] private DamageNumber _damageNumberPrefab;
        [SerializeField] private DamageNumber _healNumberPrefab;

        [Header("Events")]
        [SerializeField] private UnityEvent _onGuard;
        [SerializeField] private UnityEvent _onAttack;

        #endregion

        #region Private Fields

        private CharacterControl _control;
        private Animator _animator;
        private Player.PlayerInput _playerInput;

        // Guard state
        private float[] _guardCooldowns;
        private List<float> _guardHealths;
        private int _maxGuardHealth;
        private bool _isCrouching;

        // Parry state
        private float _parryTimer;
        private bool _isParried;

        // Animator hashes
        private static readonly int AttackHash        = Animator.StringToHash("Attack1");
        private static readonly int AttackIndexHash   = Animator.StringToHash("AttackIndex");
        private static readonly int GuardHash         = Animator.StringToHash("Guard");
        private static readonly int SpeedHash         = Animator.StringToHash("Speed");
        private static readonly int KnockOutHash      = Animator.StringToHash("KnockOut");
        private static readonly int ChargeReleaseHash = Animator.StringToHash("ChargeRelease");
        private static readonly int ChargeLevelHash   = Animator.StringToHash("ChargeLevel");

        // Charge state
        private float _chargeDamageMultiplier = 1.0f;

        // Combo state
        private int _comboCount;
        private float _comboTimer;
        private bool _isComboWindowOpen;

        #endregion

        #region Public Properties

        public bool IsCrouching => _isCrouching;
        public ParticleSystem HitEffect => _hitEffect;
        public float HitStopTime => _hitStopTime;
        public int ComboCount => _comboCount;
        public bool IsComboWindowOpen => _isComboWindowOpen;

        #endregion

        #region Events

        /// <summary>ダメージを受けた時に発火</summary>
        public event System.Action<float> OnDamageReceived;

        /// <summary>回復した時に発火</summary>
        public event System.Action<float> OnHealReceived;

        /// <summary>死亡した時に発火</summary>
        public event System.Action OnDeath;

        /// <summary>ガードが破壊された時に発火</summary>
        public event System.Action<int> OnGuardBroken;

        /// <summary>ガードが回復した時に発火</summary>
        public event System.Action<int> OnGuardRecovered;

        #endregion

        #region Initialization

        public void Initialize(CharacterControl control, Animator animator, Player.PlayerInput playerInput, int guardNum, int guardHealth)
        {
            _control = control;
            _animator = animator;
            _playerInput = playerInput;
            _maxGuardHealth = guardHealth;

            _guardCooldowns = new float[guardNum];
            _guardHealths = new List<float>();
            for (int i = 0; i < guardNum; i++)
            {
                _guardHealths.Add(guardHealth);
            }
        }

        #endregion

        #region Public Methods - Guard

        /// <summary>
        /// ガード処理（FixedUpdateから呼び出し）
        /// </summary>
        public void ProcessGuard(bool guardInput)
        {
            var animState = _animator.GetCurrentAnimatorStateInfo(0);

            if (!guardInput)
            {
                if (animState.IsName("Guard"))
                {
                    _animator.SetFloat(SpeedHash, 1.0f);
                }
                _isCrouching = false;
                return;
            }

            _isCrouching = true;

            if (!animState.IsName("Guard"))
            {
                _animator.SetTrigger(GuardHash);
                _onGuard?.Invoke();
            }
            else
            {
                if (animState.normalizedTime > GameBalance.GUARD_ANIMATION_STOP_TIME)
                {
                    _animator.SetFloat(SpeedHash, 0.0f);
                }
            }
        }

        /// <summary>
        /// ガードクールダウンの減少
        /// </summary>
        public void DecreaseGuardCooldowns(float deltaTime)
        {
            if (_guardCooldowns == null) return;

            for (int i = 0; i < _guardCooldowns.Length; i++)
            {
                if (_guardCooldowns[i] <= 0f) continue;

                _guardCooldowns[i] -= deltaTime;
                if (_guardCooldowns[i] < 0f)
                {
                    _guardCooldowns[i] = 0f;
                    _guardHealths[i] = _maxGuardHealth;
                    OnGuardRecovered?.Invoke(i);
                }
                break;
            }
        }

        /// <summary>
        /// ガードによるダメージ軽減。パリィ窓内なら完全遮断してパリィ成功フラグを立てる。
        /// </summary>
        public float GuardInflicted(float damage, float cooldown)
        {
            // パリィ判定: ガード入力直後の窓内に攻撃を受けた場合はパリィ成功
            if (_parryTimer > 0f)
            {
                _isParried = true;
                return 0f;
            }

            void GuardBreak(int index)
            {
                _guardHealths[index] = 0;
                _guardCooldowns[index] = cooldown;
                OnGuardBroken?.Invoke(index);
            }

            if (_maxGuardHealth == 1)
            {
                for (int i = 0; i < _guardHealths.Count; i++)
                {
                    if (_guardHealths[i] == 0) continue;
                    GuardBreak(i);
                    return 0;
                }
                return damage;
            }

            for (int i = 0; i < _guardHealths.Count; i++)
            {
                damage -= _guardHealths[i];
                if (damage < 0)
                {
                    _guardHealths[i] = -damage;
                    return 0;
                }
                GuardBreak(i);
            }
            return damage;
        }

        /// <summary>
        /// 最小ガードクールダウンの取得
        /// </summary>
        public float GetMinGuardCooldown()
        {
            if (_guardCooldowns == null || _guardCooldowns.Length == 0) return 0f;

            float min = float.MaxValue;
            foreach (var cd in _guardCooldowns)
            {
                if (cd < min) min = cd;
            }
            return min;
        }

        /// <summary>
        /// パリィ窓を開始する。GuardState.OnEnterから呼び出す。
        /// </summary>
        public void StartParryWindow()
        {
            _parryTimer = GameBalance.PARRY_WINDOW;
            _isParried = false;
        }

        /// <summary>
        /// パリィタイマーを減算する。GuardState.OnUpdateから呼び出す。
        /// </summary>
        public void DecreaseParryTimer(float deltaTime)
        {
            if (_parryTimer > 0f)
                _parryTimer -= deltaTime;
        }

        /// <summary>
        /// パリィ成功を消費する。成功していれば true を返し、フラグをリセットする。
        /// </summary>
        public bool ConsumeParry()
        {
            if (!_isParried) return false;
            _isParried = false;
            return true;
        }

        #endregion

        #region Public Methods - Attack

        /// <summary>
        /// 攻撃モーション再生
        /// </summary>
        public void AttackMotion(bool isOnUI, bool attackInput)
        {
            if (isOnUI || !attackInput) return;

            var state = _animator.GetCurrentAnimatorStateInfo(0);
            if (IsAttackAnimation(state) && !_isComboWindowOpen) return;

            if (_comboCount >= 10)
                _comboCount = 1;
            else
                _comboCount++;

            _isComboWindowOpen = false;
            _animator.SetInteger(AttackIndexHash, _comboCount);
            _animator.SetTrigger(AttackHash);
            _onAttack?.Invoke();
        }

        /// <summary>
        /// コンボタイマーの更新（Updateから呼び出し）
        /// </summary>
        public void UpdateComboTimer(float deltaTime)
        {
            if (_animator == null) return;

            var state = _animator.GetCurrentAnimatorStateInfo(0);
            bool isAttacking = IsAttackAnimation(state);

            if (isAttacking)
            {
                if (state.normalizedTime >= GameBalance.COMBO_WINDOW_OPEN_NORMALIZED_TIME)
                {
                    _isComboWindowOpen = true;
                    _comboTimer = GameBalance.COMBO_WINDOW_TIME;
                }
            }
            else if (_isComboWindowOpen)
            {
                _comboTimer -= deltaTime;
                if (_comboTimer <= 0)
                    ResetCombo();
            }
            else if (_comboCount > 0)
            {
                ResetCombo();
            }
        }

        /// <summary>
        /// 現在のコンボに対応するダメージ倍率を返す
        /// </summary>
        public float GetComboDamageMultiplier()
        {
            if (_comboCount == 0) return 1.0f;
            int index = Mathf.Clamp(_comboCount - 1, 0, GameBalance.COMBO_DAMAGE_MULTIPLIERS.Length - 1);
            return GameBalance.COMBO_DAMAGE_MULTIPLIERS[index];
        }

        /// <summary>
        /// チャージ攻撃を発動する。チャージ段階をAnimatorに渡し、ダメージ倍率を設定する。
        /// </summary>
        public void ChargeAttackMotion(int chargeLevel)
        {
            int clamped = Mathf.Clamp(chargeLevel, 0, GameBalance.CHARGE_DAMAGE_MULTIPLIERS.Length - 1);
            _chargeDamageMultiplier = GameBalance.CHARGE_DAMAGE_MULTIPLIERS[clamped];
            _animator.SetInteger(ChargeLevelHash, chargeLevel);
            _animator.SetTrigger(ChargeReleaseHash);
        }

        /// <summary>
        /// チャージ攻撃のダメージ倍率を返し、1.0にリセットする（DamageManagerから呼び出し）。
        /// </summary>
        public float GetChargeDamageMultiplier()
        {
            float m = _chargeDamageMultiplier;
            _chargeDamageMultiplier = 1.0f;
            return m;
        }

        #endregion

        #region Public Methods - Damage

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void DamageInflicted(float damage, Vector3 effectPosition, float killerLuck, float knockbackPower,
            ref float currentHealth, ref List<float> extraHealths, System.Action<float> onKillerLuckSet)
        {
            // 追加HPがある場合の処理
            if (extraHealths.Count > 0)
            {
                for (int i = extraHealths.Count - 1; i >= 0; i--)
                {
                    if (extraHealths[i] <= damage)
                    {
                        damage -= extraHealths[i];
                        extraHealths.RemoveAt(i);
                        if (damage <= 0)
                        {
                            damage = 0;
                            break;
                        }
                    }
                    else
                    {
                        extraHealths[i] -= damage;
                        damage = 0;
                        break;
                    }
                }
            }

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                onKillerLuckSet?.Invoke(killerLuck);
            }

            // ダメージ数値表示
            if (_damageNumberPrefab != null)
            {
                Vector3 pos = new Vector3(transform.position.x, transform.position.y + GameBalance.DAMAGE_NUMBER_Y_OFFSET, 0f);
                _damageNumberPrefab.Spawn(pos, damage);
            }

            // ダメージモーション
            DamageMotion(effectPosition - transform.position, knockbackPower);
            _playerInput?.GenerateImpulse();

            OnDamageReceived?.Invoke(damage);
        }

        /// <summary>
        /// 回復を受ける
        /// </summary>
        public void HealInflicted(float heal, bool overHeal, Vector3 effectPosition,
            ref float currentHealth, float maxHealth, ref List<float> extraHealths)
        {
            if (!overHeal)
            {
                float maxHeal = maxHealth - currentHealth;
                heal = Mathf.Clamp(heal, 0, maxHeal);
                currentHealth += heal;
            }
            else
            {
                float healable = maxHealth - currentHealth;
                if (healable < heal)
                {
                    float overHealValue = heal - (maxHealth - currentHealth);
                    currentHealth = maxHealth;

                    for (int i = 0; i < System.Math.Ceiling(overHealValue / maxHealth); i++)
                    {
                        if (overHealValue > maxHealth)
                        {
                            extraHealths.Add(maxHealth);
                            overHealValue -= maxHealth;
                        }
                        else
                        {
                            extraHealths.Add(overHealValue);
                        }
                    }
                }
                else
                {
                    currentHealth += heal;
                }
            }

            // 回復数値表示
            if (_healNumberPrefab != null)
            {
                Vector3 pos = new Vector3(transform.position.x, transform.position.y + GameBalance.DAMAGE_NUMBER_Y_OFFSET, 0f);
                _healNumberPrefab.Spawn(pos, heal);
            }

            OnHealReceived?.Invoke(heal);
        }

        /// <summary>
        /// 死亡判定
        /// </summary>
        public bool CheckDead(float currentHealth)
        {
            if (currentHealth > 0) return false;

            _animator.SetTrigger(KnockOutHash);
            OnDeath?.Invoke();
            Destroy(gameObject, GameBalance.DEATH_DESTROY_DELAY);
            return true;
        }

        #endregion

        #region Internal Methods

        /// <summary>PlayerCharacterController が入力切り替え時に呼び出す。</summary>
        internal void SetPlayerInput(Player.PlayerInput playerInput) => _playerInput = playerInput;

        #endregion

        #region Private Methods

        private bool IsAttackAnimation(AnimatorStateInfo state)
        {
            return state.IsName("Attack1") || state.IsName("Attack10_Finisher");
        }

        private void ResetCombo()
        {
            _comboCount = 0;
            _isComboWindowOpen = false;
            _comboTimer = 0;
        }

        private void DamageMotion(Vector3 direction, float knockbackPower)
        {
            if (_control.GetIsDead()) return;

            var seq = DOTween.Sequence();
            seq.Append(transform.DOShakePosition(_hitStopTime, GameBalance.DAMAGE_SHAKE_STRENGTH, GameBalance.DAMAGE_SHAKE_VIBRATO, fadeOut: false));

            if (knockbackPower > 0)
            {
                // 物理ベースのノックバック
                _control.ApplyKnockback(direction, knockbackPower);
            }
            else
            {
                // 既存の固定距離ノックバック（DOTween演出）
                var backPosition = transform.position + direction.normalized * GameBalance.KNOCKBACK_DISTANCE;
                seq.Append(transform.DOMove(backPosition, GameBalance.KNOCKBACK_DURATION));
            }

            _animator.CrossFade("Damaged", 0f, 0, GameBalance.DAMAGE_MOTION_BLEND_START);
        }

        #endregion
    }
}
