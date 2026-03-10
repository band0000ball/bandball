using Databrain;
using System;
using Databrain.Attributes;
using Skill.Component;

namespace Skill.SkillData
{
    [Serializable]
    public abstract class SkillData : DataObject
    {
        [DatabrainSerialize] public int skillID;
        [DatabrainSerialize] public string skillName;
        
        [DatabrainSerialize] public int ownerTag;
        [DatabrainSerialize] public int popNum = 1;
        [DatabrainSerialize] public SkillComponent prefab;
        [DatabrainSerialize] public float staminaConsume = 1f;
        [DatabrainSerialize] public float cooldownTime = 1f;
        [DatabrainSerialize] public float minCooldownTime = 0.1f;
        [DatabrainSerialize] public float minRange = 0.1f;
        [DatabrainSerialize] public float maxRange = 2f;
        [DatabrainSerialize] public float size = 1f;
        [DatabrainSerialize] public bool isAuto = false;

        public bool Use()
        {
            throw new NotImplementedException();
        }
    }
}