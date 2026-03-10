using System;
using Commons;
using Databrain.Attributes;
using Manager;
using StatusEffect;
using UnityEngine;

namespace Skill.SkillData
{
    [Serializable]
    public class AttackSkillData : SkillData
    {
        [DatabrainSerialize] public AttributeMagnification.Attribute attribute = AttributeMagnification.Attribute.None;
        [DatabrainSerialize] public float attributeValue = 1f;
        [DatabrainSerialize] public float attackPower = 1f;
        [DatabrainSerialize] public float speed = 0.1f;
        [DatabrainSerialize] public float damageOverTime = 0f;
        [DatabrainSerialize] public float duration = 0f;
        [DatabrainSerialize] public bool isFixedDamage = false;
        [DatabrainSerialize] public bool isLockOn = false;
        [DatabrainSerialize] public bool isHeal = false;
        [DatabrainSerialize] public bool isOverHeal = false;
        [DatabrainSerialize] public DurationData.DurationType stopDurationType = DurationData.DurationType.None;
        [DatabrainSerialize] public float knockbackPower = 0f;
        [DatabrainSerialize] public KnockbackTarget knockbackTarget = KnockbackTarget.Enemy;
        [DatabrainSerialize] public StatusEffectKind statusEffectType = StatusEffectKind.None;
        [DatabrainSerialize] public float statusEffectDuration = 0f;
        [DatabrainSerialize] public float statusEffectChance = 0f;
    }
}