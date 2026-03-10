/*This script created by using docs.unity3d.com/ScriptReference/MonoBehaviour.OnParticleCollision.html*/
using System;
using System.Collections.Generic;
using Character;
using Commons;
using Manager;
using Skill.SkillData;
using StatusEffect;
using UnityEngine;
using CharacterControl = Character.CharacterControl;

namespace Skill.Component
{
    [Serializable]
    public class SkillComponent : MonoBehaviour
    {
        public ParticleSystem[] effectsOnCollision;
        public CharacterControl parent;
        public float power;
        public float speed;
        public AttributeMagnification.Attribute attribute;
        public float attributeValue;
        public float criticalChance;
        public float criticalDamage;
        public float shieldAttackRate;
        public float durationTime;
        public float durationDamage;
        public bool isFixedDamage;
        public bool isOverHeal;
        public DurationData.DurationType stopDurationType;
        public float knockbackPower;
        public KnockbackTarget knockbackTarget;
        public StatusEffectKind statusEffectType;
        public float statusEffectDuration;
        public float statusEffectChance;

        public float destroyTimeDelay = GameBalance.EFFECT_DESTROY_DELAY;
        public float offset;
        public Vector3 rotationOffset = new(0, 0, 0);
        public bool useWorldSpacePosition;
        public bool useOnlyRotationOffset = true;
        public bool useFirePointRotation;
        public bool destroyMainEffect;
        private ParticleSystem _part;
        private List<ParticleCollisionEvent> _collisionEvents = new();
        private CharacterControl _target;

        void Start()
        {
            _part = GetComponent<ParticleSystem>();
            parent = transform.parent.GetComponent<CharacterControl>();
            var effectMain = _part.main;
            effectMain.simulationSpeed = speed;
        }

        void Update()
        {
            if (_target is null) return;
            transform.rotation = Quaternion.LookRotation(_target.transform.position - transform.position, Vector3.up);
        }

        void OnParticleCollision(GameObject other)
        {
            if (other.layer == LayerMask.NameToLayer("Player") || other.layer == LayerMask.NameToLayer("Enemy"))
            {
                Transform otherParent = other.transform;
                UnityEngine.Component character;
                while (!otherParent.TryGetComponent(typeof(CharacterControl), out character))
                {
                    otherParent = otherParent.parent;
                    if (otherParent is null) return;
                }
                CharacterControl enemy = (CharacterControl)character;
                DamageManager.Damage(enemy, this);
            }
            else if (other.layer == LayerMask.NameToLayer("Shield"))
            {
                GuardComponent guard = (GuardComponent)other.GetComponentInChildren(typeof(GuardComponent));
                DamageManager.GuardDamage(guard, this);
            }
            else return;

            int numCollisionEvents = _part.GetCollisionEvents(other, _collisionEvents);
            for (int i = 0; i < numCollisionEvents; i++)
            {
                foreach (var effect in effectsOnCollision)
                {
                    var effectMain = effect.main;
                    effectMain.simulationSpeed = speed;
                    var instance = Instantiate(effect, _collisionEvents[i].intersection + _collisionEvents[i].normal * offset, new Quaternion());
                    if (!useWorldSpacePosition) instance.transform.parent = transform;
                    if (useFirePointRotation) { instance.transform.LookAt(transform.position); }
                    else if (rotationOffset != Vector3.zero && useOnlyRotationOffset) { instance.transform.rotation = Quaternion.Euler(rotationOffset); }
                    else
                    {
                        instance.transform.LookAt(_collisionEvents[i].intersection + _collisionEvents[i].normal);
                        instance.transform.rotation *= Quaternion.Euler(rotationOffset);
                    }
                    Destroy(instance, destroyTimeDelay);
                }
            }
            if (destroyMainEffect)
            {
                Destroy(gameObject, destroyTimeDelay + GameBalance.MAIN_EFFECT_DESTROY_ADDITIONAL_DELAY);
            }
        }

        public void Initialize(CharacterControl character, AttackSkillData skillData, CharacterControl target = null)
        {
            power = character.GetAttackPower() + skillData.attackPower;
            power = skillData.isHeal ? -1 * power : power;
            speed = skillData.speed;
            attribute = skillData.attribute;
            attributeValue = skillData.attributeValue + character.GetAttributeValue(skillData.attribute);
            criticalChance = character.GetLuck();
            criticalDamage = character.GetCriticalDamage();
            shieldAttackRate = character.GetShieldAttackRate();
            stopDurationType = skillData.stopDurationType;
            if (durationTime > 0)
            {
                durationTime = skillData.duration;
                durationDamage = character.GetAttackPower() * GameBalance.DOT_BASE_MULTIPLIER + skillData.damageOverTime;
                durationDamage = skillData.isHeal ? -1 * durationDamage : durationDamage;
                isFixedDamage = skillData.isFixedDamage;
            }
            isOverHeal = skillData.isOverHeal;
            knockbackPower = skillData.knockbackPower;
            knockbackTarget = skillData.knockbackTarget;
            statusEffectType = skillData.statusEffectType;
            statusEffectDuration = skillData.statusEffectDuration;
            statusEffectChance = skillData.statusEffectChance;
            _target = target;
        }

        public float GetPower() => power;
    }
}
