using System;
using UnityEngine;

namespace Items.ItemData
{
    [Serializable]
    [CreateAssetMenu(fileName = "ValuableItemData", menuName = "ScriptableObject/Data/Item/ValuableItemData")]
    public class ValuableItemData : OwnedItemData
    {
        public enum ValuableCategory
        {
            Artifact,
            Jewelry,
            Fragile,
            Document,
        }
        
        // 貴重品系の基本データ
        public ValuableCategory category;
    }
}