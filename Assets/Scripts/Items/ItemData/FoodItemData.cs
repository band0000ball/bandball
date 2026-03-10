using System;
using System.Collections.Generic;
using Character;
using Commons;
using Buff;
using UnityEngine;

namespace Items.ItemData
{
    [Serializable]
    [CreateAssetMenu(fileName = "FoodItemData", menuName = "ScriptableObject/Data/Item/FoodItemData")]
    public class FoodItemData : OwnedItemData
    {
        public enum FoodCategory
        {
            Raw,
            Cooked,
            Preserved,
            Frozen,
        }
        
        // 食料系アイテムの基本データ
        public FoodCategory category;
        public float buffTime = 1f;
        public List<int> buffTypes = new();
        public List<float> buffValues = new();

        public override bool Use(CharacterControl ctl)
        {
            switch (category)
            {
                case FoodCategory.Raw:
                    // todo 一定確率でデバフ
                    break;
                case FoodCategory.Cooked:
                    break;
                case FoodCategory.Preserved:
                    // todo 
                    break;
                case FoodCategory.Frozen:
                    AttributeMagnification.Attribute attribute = ctl.GetMaxAttribute().attribute;
                    if (attribute is not AttributeMagnification.Attribute.Frame and AttributeMagnification.Attribute.Aqua) return false;
                    break;
            }

            int count = buffTypes.Count > buffValues.Count ? buffValues.Count : buffTypes.Count;
            for (int i = 0; i < count; i++)
            {
                ctl.AddBuff((BuffType)buffTypes[i], buffValues[i], buffTime);
            }
            return true;
        }
    }
}