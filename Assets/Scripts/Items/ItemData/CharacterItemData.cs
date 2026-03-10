using System;
using Character;
using UnityEngine;

namespace Items.ItemData
{
    [Serializable]
    [CreateAssetMenu(fileName = "CharacterItemData", menuName = "ScriptableObject/Data/Item/CharacterItemData")]
    public class CharacterItemData : HasUniqueIdData
    {
        // キャラクターの基本データ
        public MetaStatus meta;
    }
}