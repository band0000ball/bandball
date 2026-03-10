using System.Collections.Generic;
using System.Linq;
using Character;
using Commons;
using Skill.Component;
using Skill.SkillData;
using StatusEffect;
using StatusEffect.Effects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Manager
{
    public class DamageManager : MonoBehaviour
    {
        private static readonly List<CharacterControl> Characters = new();
        private static List<DamageData> _damage = new();
        private static readonly List<DurationData> Duration = new();

        void FixedUpdate()
        {
            if (Duration.Count == 0) return;
            List<DurationData> popItem = new List<DurationData>();
            foreach (var durationData in Duration.Select((x, i) => new { x, i }))
            {
                DurationDamage(durationData.x);
                if (!durationData.x.DecreaseDuration(Time.deltaTime)) popItem.Add(durationData.x);
            }
            Duration.RemoveAll((x) => popItem.Contains(x));
        }

        public static void AddRequest(CharacterControl character)
        {
            Characters.Add(character);
        }

        /// <summary>
        /// 指定したキャラクターをリストから削除する
        /// </summary>
        /// <param name="character">削除するキャラクター</param>
        public static void RemoveCharacter(CharacterControl character)
        {
            int idx = Characters.IndexOf(character);
            if (idx < 0) return;

            Characters.RemoveAt(idx);
            // このキャラクターに関連する継続ダメージも削除
            Duration.RemoveAll(d => d.AttackerIdx == idx || d.DefenderIdx == idx);
        }

        /// <summary>
        /// 全ての静的リストをクリアする
        /// シーン遷移時に呼び出すこと
        /// </summary>
        public static void ClearAll()
        {
            Characters.Clear();
            _damage.Clear();
            Duration.Clear();
        }

        public static void GuardDamage(GuardComponent guard, SkillComponent skill)
        {
            int attackerIdx = Characters.IndexOf(skill.parent);
            int defenderIdx = Characters.IndexOf(guard.parent);
            Vector3 position = Vector3.zero;

            if (skill.power > 0)
            {
                float damage = CalcAttack(
                    skillPower: skill.power,
                    defenderLuck: guard.GetLuck(),
                    criticalChance: skill.criticalChance,
                    criticalDamage: skill.criticalDamage);
                damage = CalcDefence(
                    baseDamage: damage,
                    attribute: skill.attribute,
                    attributeValue: skill.attributeValue,
                    defender: guard.parent);
                damage = guard.DamageInflicted(damage: damage);
            
                DamageData dd = new DamageData(
                    baseDamage: damage,
                    attackerIdx: attackerIdx,
                    defenderIdx: defenderIdx,
                    attribute: 0,
                    attributeValue: 0,
                    damageType: DamageData.DamageType.Damage
                );
                
                _damage.Add(dd);
                
                if (damage > 0)
                {
                    position = skill.transform.position;
                    float effectiveKnockback = CalcKnockbackPower(skill.knockbackPower, skill.knockbackTarget, guard.parent);
                    guard.parent.DamageInflicted(
                        damage: damage,
                        effectPosition: position,
                        killerLuck: skill.parent.GetLuck(),
                        knockbackPower: effectiveKnockback);
                }
            }

            if (skill.durationTime > 0)
            {
                AddDuration(
                    attackerIdx: attackerIdx, 
                    defenderIdx: defenderIdx,
                    position: position,
                    skill: skill
                );
            }
        }

        public static void Damage(CharacterControl defender, SkillComponent skill)
        {
            int attackerIdx = Characters.IndexOf(skill.parent);
            int defenderIdx = Characters.IndexOf(defender);
            Vector3 position = skill.transform.position;

            if (skill.power > 0)
            {
                float comboMultiplier  = skill.parent != null ? skill.parent.GetComboDamageMultiplier()  : 1.0f;
                float chargeMultiplier = skill.parent != null ? skill.parent.GetChargeDamageMultiplier() : 1.0f;
                float damage = CalcAttack(
                skillPower: skill.power * comboMultiplier * chargeMultiplier,
                defenderLuck: defender.GetLuck(),
                criticalChance: skill.criticalChance,
                criticalDamage: skill.criticalDamage);
                damage = CalcDefence(
                    baseDamage: damage,
                    attribute: skill.attribute,
                    attributeValue: skill.attributeValue,
                    defender: defender);
                damage = CalcShield(
                    baseDamage: damage,
                    enemyShieldAttackRate: skill.shieldAttackRate,
                    defender: defender);
            
                DamageData dd = new DamageData(
                    baseDamage: damage,
                    attackerIdx: attackerIdx,
                    defenderIdx: defenderIdx,
                    attribute: (int)skill.attribute,
                    attributeValue: skill.attributeValue,
                    damageType: DamageData.DamageType.Damage
                );
                
                _damage.Add(dd);
                
                float effectiveKnockback = CalcKnockbackPower(skill.knockbackPower, skill.knockbackTarget, defender);
                defender.DamageInflicted(
                    damage: damage,
                    effectPosition: position,
                    killerLuck: skill.parent.GetLuck(),
                    knockbackPower: effectiveKnockback);

                ApplyStatusEffectFromSkill(defender, skill.statusEffectType, skill.statusEffectDuration, skill.statusEffectChance);
            }
            else
            {
                // todo heal処理
                DamageData dd = new DamageData(
                    baseDamage: -skill.power,
                    attackerIdx: attackerIdx,
                    defenderIdx: defenderIdx,
                    attribute: (int)skill.attribute,
                    attributeValue: skill.attributeValue,
                    damageType: DamageData.DamageType.Heal
                );
                
                _damage.Add(dd);
                
                defender.HealInflicted(
                    heal: -skill.power,
                    overHeal: skill.isOverHeal,
                    effectPosition: position
                    );
            }
            StopDuration(defenderIdx, skill.stopDurationType);

            if (skill.durationTime > 0)
            {
                AddDuration(
                    attackerIdx: attackerIdx, 
                    defenderIdx: defenderIdx,
                    position: position,
                    skill: skill
                    );
            }
        }

        private static void AddDuration(int attackerIdx, int defenderIdx, Vector3 position, SkillComponent skill)
        {
            AttributeMagnification.Attribute attribute = skill.attribute;
            DamageData.DamageType durationDamageType = skill.durationDamage > 0 
                ? DamageData.DamageType.DamageOverTime : DamageData.DamageType.HealOverTime;
            float luck = skill.parent.GetLuck();
            float attributeValue = Mathf.Abs(skill.durationDamage);
            float duration = skill.durationTime;
            bool isFixedDamage = skill.isFixedDamage;
            bool overHeal = skill.isOverHeal;
            DurationData.DurationType stopDurationType = skill.stopDurationType;
            DurationData dd = new DurationData(
                attackerIdx: attackerIdx,
                defenderIdx: defenderIdx,
                attribute: attribute,
                durationDamageType: durationDamageType,
                position: position,
                luck: luck,
                duration: duration,
                value: attributeValue,
                isFixedDamage: isFixedDamage,
                overHeal: overHeal,
                stopDurationType: stopDurationType
            );
            dd.SetDuration(skill.durationTime);
            Duration.Add(dd);
        }
        
        static void StopDuration(int defenderIdx, DurationData.DurationType stopDurationType)
        {
            if (stopDurationType is DurationData.DurationType.None) return;
            Duration.RemoveAll(d => d.DefenderIdx == defenderIdx && d.StopDurationType == stopDurationType);
        }

        private void DurationDamage(DurationData dd)
        {
            CharacterControl character = Characters[dd.DefenderIdx];
            if (dd.DurationDamageType == DamageData.DamageType.DamageOverTime)
            {
                float damage = dd.Value;
                if (!dd.IsFixedDamage) damage = CalcDefence(0, dd.Attribute, damage, character);
                character.DamageInflicted(
                    damage: damage,
                    effectPosition: dd.Position,
                    killerLuck: dd.Luck
                );
            }
            else
            {
                // todo Heal処理
                character.HealInflicted(
                    heal: dd.Value,
                    overHeal: dd.OverHeal,
                    effectPosition: dd.Position);
            }
            
            _damage.Add(new DamageData(
                baseDamage: 0, 
                attackerIdx: dd.AttackerIdx,
                defenderIdx: dd.DefenderIdx,
                attribute: (int)dd.Attribute,
                attributeValue: dd.Value,
                damageType: dd.DurationDamageType
            ));
        }

        /// <summary>
        /// ノックバック対象に応じて有効なノックバック力を計算する
        /// </summary>
        private static float CalcKnockbackPower(float knockbackPower, KnockbackTarget knockbackTarget, CharacterControl defender)
        {
            if (knockbackPower <= 0) return 0f;

            int defenderLayer = defender.gameObject.layer;
            bool isPlayer = defenderLayer == LayerMask.NameToLayer("Player");
            bool isEnemy = defenderLayer == LayerMask.NameToLayer("Enemy");

            return knockbackTarget switch
            {
                KnockbackTarget.Enemy when isEnemy => knockbackPower,
                KnockbackTarget.Player when isPlayer => knockbackPower,
                KnockbackTarget.Both => knockbackPower,
                _ => 0f
            };
        }

        /// <summary>
        /// スキルデータに基づいて状態異常を付与する
        /// </summary>
        private static void ApplyStatusEffectFromSkill(CharacterControl defender, StatusEffectKind effectKind, float duration, float chance)
        {
            if (effectKind == StatusEffectKind.None || duration <= 0f || chance <= 0f) return;
            if (Random.value >= chance) return;

            IStatusEffect effect = effectKind switch
            {
                StatusEffectKind.Stun       => new StunEffect(duration),
                StatusEffectKind.Depression => new DepressionEffect(duration),
                StatusEffectKind.Bleeding   => new BleedingEffect(duration),
                StatusEffectKind.Anxiety    => new AnxietyEffect(duration),
                _ => null
            };

            if (effect != null) defender.AddStatusEffect(effect);
        }

        private static float CalcAttack(float skillPower, float defenderLuck, float criticalChance, float criticalDamage)
        {
            // ダメージ = 攻撃力 * 基本倍率 * ランダム値 * クリティカル倍率
            float damage = skillPower * GameBalance.DAMAGE_BASE_MULTIPLIER;
            // luck値によってダメージ量をランダムに変化させる
            float randomValue = Random.Range(-(defenderLuck + 1f), 0f);
            bool isCritical = randomValue > -criticalChance;
            randomValue *= GameBalance.RANDOM_DAMAGE_MULTIPLIER;
            randomValue += 1f;
            damage *= randomValue;
            // クリティカル時はダメージ倍率を加算
            damage = isCritical ? damage * (1f + criticalDamage * GameBalance.CRITICAL_DAMAGE_MULTIPLIER) : damage;
            return damage;
        }
        
        private static float CalcShield(float baseDamage, float enemyShieldAttackRate, CharacterControl defender)
        {
            float shieldDamage = baseDamage * enemyShieldAttackRate * GameBalance.SHIELD_DAMAGE_MULTIPLIER;
            float shield = defender.GetShield();
            baseDamage -= shield;
            baseDamage = baseDamage < 0 ? 0 : Tools.RoundValue(baseDamage, 3);
            if (shield > 0) defender.DecreaseShield(shieldDamage);
            return baseDamage;
        }
        
        private static float CalcDefence(float baseDamage, AttributeMagnification.Attribute attribute, float attributeValue, CharacterControl defender)
        {
            // 防御力で低減させる
            baseDamage -= defender.GetDefencePower() * GameBalance.DEFENSE_REDUCTION_MULTIPLIER;
            // 属性計算
            var att = defender.GetMaxAttribute();
            float attDamage = attributeValue - AttributeMagnification.Choice(att.attribute, attribute) * att.value;
            attDamage *= GameBalance.ATTRIBUTE_DAMAGE_MULTIPLIER;
            baseDamage += attDamage;
            return baseDamage;
        }
    }

    public class DamageData
    {
        public enum DamageType
        {
            Heal,
            Damage,
            HealOverTime,
            DamageOverTime,
            WithGuard,
            WithBuff,
            WithDeBuff
        }
        
        private float _baseDamage;
        private int _attackerIdx;
        private int _defenderIdx;
        private int _attribute;
        private float _attributeValue;
        private DamageType _damageType;

        public DamageData(float baseDamage, int attackerIdx, int defenderIdx,
            int attribute, float attributeValue, DamageType damageType)
        {
            _baseDamage = baseDamage;
            _attackerIdx = attackerIdx;
            _defenderIdx = defenderIdx;
            _attribute = attribute;
            _attributeValue = attributeValue;
            _damageType = damageType;
        }
    }

    public class DurationData
    {
        public enum DurationType
        {
            None,
            Damage,
            Heal,
            Buff,
            DeBuff
        }
        
        private float _duration;
        
        public int AttackerIdx { get; }
        public int DefenderIdx { get; }
        public AttributeMagnification.Attribute Attribute { get; }

        public DamageData.DamageType DurationDamageType { get; }
        public Vector3 Position { get; }
        public float Luck { get; }
        public float Value { get; }
        public bool IsFixedDamage { get; }
        public bool OverHeal { get; }
        public DurationType StopDurationType { get; }

        public DurationData(int attackerIdx, int defenderIdx, AttributeMagnification.Attribute attribute, DamageData.DamageType durationDamageType, Vector3 position, float luck, float duration, float value, bool isFixedDamage, bool overHeal, DurationType stopDurationType)
        {
            AttackerIdx = attackerIdx;
            DefenderIdx = defenderIdx;
            Attribute = attribute;
            DurationDamageType = durationDamageType;
            Position = position;
            Luck = luck;
            _duration = duration;
            Value = value;
            IsFixedDamage = isFixedDamage;
            OverHeal = overHeal;
            StopDurationType = stopDurationType;
        }

        public void SetDuration(float time) { _duration = time; }

        public bool DecreaseDuration(float time)
        {
            _duration -= time;
            return _duration > 0;
        }
    }
}