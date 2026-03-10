using Databrain;
using System;
using Commons;
using Databrain.Attributes;

namespace Character
{
    [Serializable]
    public class BehaviourStatus : DataObject
    {
        public int jumpTime = 1;
        public float moveSpeed = 1f;
        public float jumpForce = 1f;
        public float jumpSpeed = 1f;
        /*吹き飛ばし耐性*/
        public float corePower = 1f;
        public float strength = 1f;
        public float control = 1f;
        public float thickness = 1f;
        /*状態異常耐性*/
        public float endurance = 1f;
        public float additionalGravity = 0f;
    }

    [Serializable]
    public class AttributeStatus : DataObject
    {
        public float frame;
        public float aqua;
        public float plant;
        public float electric;
        public float ground;
        public float ice;
        public float oil;
        public float toxin;
        public float wind;
        public float spirit;

        public AttributeStatus(
            float frame = 0f, 
            float aqua = 0f,
            float plant = 0f,
            float electric = 0f,
            float ground = 0f,
            float ice = 0f,
            float oil = 0f,
            float toxin = 0f,
            float wind = 0f,
            float spirit = 0f
            )
        {
            this.frame = frame;
            this.aqua = aqua;
            this.plant = plant;
            this.electric = electric;
            this.ground = ground;
            this.ice = ice;
            this.oil = oil;
            this.toxin = toxin;
            this.wind = wind;
            this.spirit = spirit;
        }

        public (float value, AttributeMagnification.Attribute attribute) MaxAttribute()
        {
            var argmax = Tools.MaxAndArg(frame, aqua, plant, electric, ground, ice, oil, toxin, wind, spirit);
            return (argmax.max, (AttributeMagnification.Attribute)argmax.arg);
        }

        public float[] AsArray()
        {
            return new[] { frame, aqua, plant, electric, ground, ice, oil, toxin, wind, spirit };
        }
    }
    
    [Serializable]
    public class BattleStatus : DataObject
    {
        public float shield = 1f;
        public float health = 1f;
        public float stamina = 1f;
        public float guardHealth = 1f;
        public float rate = 1f;
        public float shieldAttackRate = 1f;
        public float attackPower = 1f;
        public float defencePower = 1f;
        public float baseAttributePower = 1f;
        public float baseResistancePower = 1f;
        public float minRange = 1f;
        public float maxRange = 1f;
        public float diffusionRate = 1f;
        public float criticalDamage = 1f;
        public float criticalChance = 1f;
        public int guardNum = 1;
        
        #if UNITY_EDITOR
        public BattleStatus Clone(BattleStatus status)
        {
            shield = status.shield;
            health = status.health;
            stamina = status.stamina;
            guardHealth = status.guardHealth;
            rate = status.rate;
            shieldAttackRate = status.shieldAttackRate;
            attackPower = status.attackPower;
            defencePower = status.defencePower;
            baseAttributePower = status.baseAttributePower;
            baseResistancePower = status.baseResistancePower;
            minRange = status.minRange;
            maxRange = status.maxRange;
            diffusionRate = status.diffusionRate;
            criticalDamage = status.criticalDamage;
            criticalChance = status.criticalChance;
            guardNum = status.guardNum;
            return this;
        }
        #endif
    }

    [Serializable]
    public class MetaStatus : DataObject
    {
        public int id;
        public int level = 1;
        public int needForNextExp;
        public int totalExp;
        public int itemDropNum = 1;
        public float itemDropRate;
        public float luck;
        public bool isDrop;
        public bool isBoss;
        public bool isFacingRight;
        [DataObjectDropdown]
        public BehaviourStatus behaviour;
        [DataObjectDropdown]
        public AttributeStatus attribute;
        [DataObjectDropdown]
        public BattleStatus battle;
    }
}