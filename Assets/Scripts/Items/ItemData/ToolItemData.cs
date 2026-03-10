using System;
using UnityEngine;

namespace Items.ItemData
{
    [Serializable]
    [CreateAssetMenu(fileName = "ToolItemData", menuName = "ScriptableObject/Data/Item/ToolItemData")]
    public class ToolItemData : ExpendableItemData
    {
        public enum ToolCategory
        {
            Enhance,
            Repair,
            Cook,
            Work,
        }
        
        // 道具系の基本データ
        public ToolCategory category;
    }
}