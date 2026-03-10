using System;
using Buff;
using UnityEngine;

namespace Skill.SkillData
{
    [Serializable]
    [CreateAssetMenu(fileName = "BuffSkillSO", menuName = "ScriptableObject/Data/Skill/BuffSkillSO", order = 1)]
    public class BuffSkillData : SkillData
    {
        [SerializeField] public BuffType buffType;
        // デバフはbuffPowerがマイナス
        [SerializeField] public float buffPower = 0.1f;
        [SerializeField] public float duration = 0f;
        [SerializeField] public bool onlyEnemy = false;
    }
}