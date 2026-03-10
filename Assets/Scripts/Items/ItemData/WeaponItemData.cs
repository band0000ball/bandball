using System;
using Databrain.Attributes;
using Skill.SkillData;
using UnityEngine;

namespace Items.ItemData
{
    [Serializable]
    [CreateAssetMenu(fileName = "WeaponItemData", menuName = "ScriptableObject/Data/Item/WeaponItemData")]
    public class WeaponItemData : ExpendableItemData
    {
        public enum WeaponCategory
        {
            Short,
            Middle,
            Long,
            Bomb,
        }
        
        // 武器系アイテムの基本データ
        public WeaponCategory category;
        public float attackPower = 1f;
        public float rate = 1f;
        public float speed = 1f;
        public float staminaConsume = 1f;
        public int popNum = 1;
        public int attribute = 0;
        public float attributePower = 1f;
        public bool isLockOn = false;
        public bool isAuto = false;
        public EnergyItemData energyData = null;
    }
}