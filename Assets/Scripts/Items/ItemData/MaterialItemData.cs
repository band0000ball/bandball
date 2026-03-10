using System;
using UnityEngine;

namespace Items.ItemData
{
    [Serializable]
    [CreateAssetMenu(fileName = "MaterialItemData", menuName = "ScriptableObject/Data/Item/MaterialItemData")]
    public class MaterialItemData : OwnedItemData
    {
        public enum MaterialCategory
        {
            Paper,
            Ink,
            ColoringAgent,
            Metal,
            Wood,
            MonsterMaterial,
        }
        
        // 素材系の基本データ
        public MaterialCategory category;
    }
}