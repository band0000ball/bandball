using Databrain;
using System;
using System.Linq;
using Databrain.Attributes;
using Items.ItemData;
using UnityEngine;

namespace Monster
{
    // [Serializable]
    // public class CharacterSlotSO : DataObject
    // {
    //     [SerializeField] public CharacterSlot[] slotItemsList;
    //
    //     public CharacterSlot Select(int id, int level)
    //     {
    //         return slotItemsList.Select(x => x)
    //             .FirstOrDefault(x => x.CharacterId == id && x.CharacterLevel == level);
    //     }
    // }

    public class CharacterSlot : DataObject
    {
        [DataObjectDropdown(true)]
        [SerializeField] public OwnedItemData[] slotItemData = new OwnedItemData[10];
        [SerializeField] public int characterId;
        [SerializeField] public int characterLevel = 1;
    }
}