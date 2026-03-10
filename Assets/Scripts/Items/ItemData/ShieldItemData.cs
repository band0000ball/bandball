using System;
using Databrain.Attributes;
using Skill.SkillData;
using UnityEngine;

namespace Items.ItemData
{
    [Serializable]
    [CreateAssetMenu(fileName = "ShieldItemData", menuName = "ScriptableObject/Data/Item/ShieldItemData")]
    public class ShieldItemData : ExpendableItemData
    {
        public enum ShieldCategory
        {
            Head,
            Body,
            Legs,
            Hands,
        }
        
        // 盾系アイテムの基本データ
        public ShieldCategory category;
        public float shield;
        public float staminaConsume;
        public float attackedStaminaConsume;
        public int attribute;
        public float attributePower;
        public bool isAuto;
    }
}