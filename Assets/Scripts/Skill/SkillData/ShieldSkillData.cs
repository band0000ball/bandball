using System;
using UnityEngine;

namespace Skill.SkillData
{
    [Serializable]
    [CreateAssetMenu(fileName = "ShieldSkillSO", menuName = "ScriptableObject/Data/Skill/ShieldSkillSO", order = 1)]
    public class ShieldSkillData : SkillData
    {
        [SerializeField] public float shield = 1f;
        [SerializeField] public float attackedStaminaConsume = 2f;
        [SerializeField] public int attribute = 0;
        [SerializeField] public float attributeValue = 1f;
        [SerializeField] public bool withAlly = false;
    }
}