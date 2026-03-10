using Character.Interfaces;
using Commons;
using UnityEngine;

namespace Character
{
    /// <summary>
    /// キャラクターのステータスを管理するコンポーネント
    /// CharacterControlから分離された責務を担う
    /// </summary>
    public class CharacterStats : MonoBehaviour, ICharacterStats
    {
        #region Serialized Fields

        [Header("Base Stats")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _maxStamina = 50f;

        [Header("Combat Stats")]
        [SerializeField] private float _baseAttackPower = 10f;
        [SerializeField] private float _baseDefensePower = 5f;
        [SerializeField] private float _baseCriticalDamage = 1.5f;
        [SerializeField] private float _baseCriticalChance = 0.05f;
        [SerializeField] private float _baseShieldAttackRate = 1f;
        [SerializeField] private float _baseLuck = 1f;

        [Header("Regeneration")]
        [SerializeField] private float _staminaRegenRate = 1f;

        #endregion

        #region Private Fields

        private float _currentHealth;
        private float _currentStamina;
        private float _currentShield;

        // バフによる追加値（外部から設定）
        private float _buffHealth;
        private float _buffStamina;
        private float _buffAttackPower;
        private float _buffDefensePower;
        private float _buffShield;
        private float _buffCriticalDamage;
        private float _buffCriticalChance;
        private float _buffShieldAttackRate;
        private float _buffLuck;

        #endregion

        #region ICharacterStats Properties

        public float Health => _currentHealth + _buffHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0;

        public float Stamina => _currentStamina + _buffStamina;
        public float MaxStamina => _maxStamina;

        public float AttackPower => _baseAttackPower + _buffAttackPower;
        public float DefensePower => _baseDefensePower + _buffDefensePower;
        public float Shield => _currentShield + _buffShield;
        public float CriticalDamage => _baseCriticalDamage + _buffCriticalDamage;
        public float CriticalChance => _baseCriticalChance + _buffCriticalChance;
        public float ShieldAttackRate => _baseShieldAttackRate + _buffShieldAttackRate;
        public float Luck => _baseLuck + _buffLuck;

        public bool CanMove { get; set; } = true;
        public bool CanAttack { get; set; } = true;
        public bool CanGuard { get; set; } = true;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _currentStamina = _maxStamina;
        }

        #endregion

        #region ICharacterStats Methods

        public void TakeDamage(float damage)
        {
            if (damage <= 0) return;

            _currentHealth -= damage;
            _currentHealth = Mathf.Max(0, _currentHealth);
        }

        public void Heal(float amount, bool allowOverheal = false)
        {
            if (amount <= 0) return;

            if (allowOverheal)
            {
                _currentHealth += amount;
            }
            else
            {
                _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            }
        }

        public bool ConsumeStamina(float amount)
        {
            if (Stamina < amount) return false;

            _currentStamina -= amount;
            _currentStamina = Mathf.Max(0, _currentStamina);
            return true;
        }

        public void RegenerateStamina(float deltaTime)
        {
            if (_currentStamina >= _maxStamina) return;

            _currentStamina += _staminaRegenRate * deltaTime;
            _currentStamina = Mathf.Min(_maxStamina, _currentStamina);
        }

        public void DecreaseShield(float amount)
        {
            _currentShield -= amount;
            _currentShield = Mathf.Max(0, _currentShield);
        }

        #endregion

        #region Buff Management

        /// <summary>
        /// バフ値を設定する（BuffManagerから呼び出される）
        /// </summary>
        public void SetBuffValues(
            float health = 0,
            float stamina = 0,
            float attackPower = 0,
            float defensePower = 0,
            float shield = 0,
            float criticalDamage = 0,
            float criticalChance = 0,
            float shieldAttackRate = 0,
            float luck = 0)
        {
            _buffHealth = health;
            _buffStamina = stamina;
            _buffAttackPower = attackPower;
            _buffDefensePower = defensePower;
            _buffShield = shield;
            _buffCriticalDamage = criticalDamage;
            _buffCriticalChance = criticalChance;
            _buffShieldAttackRate = shieldAttackRate;
            _buffLuck = luck;
        }

        /// <summary>
        /// 個別のバフ値を更新する
        /// </summary>
        public void UpdateBuffValue(StatType statType, float value)
        {
            switch (statType)
            {
                case StatType.Health:
                    _buffHealth = value;
                    break;
                case StatType.Stamina:
                    _buffStamina = value;
                    break;
                case StatType.AttackPower:
                    _buffAttackPower = value;
                    break;
                case StatType.DefensePower:
                    _buffDefensePower = value;
                    break;
                case StatType.Shield:
                    _buffShield = value;
                    break;
                case StatType.CriticalDamage:
                    _buffCriticalDamage = value;
                    break;
                case StatType.CriticalChance:
                    _buffCriticalChance = value;
                    break;
                case StatType.ShieldAttackRate:
                    _buffShieldAttackRate = value;
                    break;
                case StatType.Luck:
                    _buffLuck = value;
                    break;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 外部データからステータスを初期化する
        /// </summary>
        public void Initialize(float maxHealth, float maxStamina, float attackPower, float defensePower, float luck)
        {
            _maxHealth = maxHealth;
            _maxStamina = maxStamina;
            _baseAttackPower = attackPower;
            _baseDefensePower = defensePower;
            _baseLuck = luck;

            _currentHealth = _maxHealth;
            _currentStamina = _maxStamina;
        }

        /// <summary>
        /// シールド値を設定する
        /// </summary>
        public void SetShield(float value)
        {
            _currentShield = value;
        }

        #endregion
    }

    /// <summary>
    /// ステータスの種類（バフ適用用）
    /// </summary>
    public enum StatType
    {
        Health,
        Stamina,
        AttackPower,
        DefensePower,
        Shield,
        CriticalDamage,
        CriticalChance,
        ShieldAttackRate,
        Luck
    }
}
