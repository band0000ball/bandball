using System;
using System.Linq;
using Character;
using Databrain;
using Databrain.Attributes;
using Databrain.Inventory;
using UnityEngine;

namespace Items.ItemData
{
    [Serializable]
    [CreateAssetMenu(fileName = "BagItemData", menuName = "ScriptableObject/Data/Item/BagItemData")]
    public class BagItemData : ExpendableItemData
    {
        // バック系アイテムの基本情報
        // public OwnedItems inventory;
        [DataObjectDropdown]
        public InventoryData inventory;
        public FoodItemData consumableItemData;
        
        private const string UserItemKey = "UserItemData";

        public void SetItemData(int itemIndex)
        {
            consumableItemData = inventory.GetSlotData(itemIndex).item as FoodItemData;
        }

        public bool Use(CharacterControl ctl)
        {
            bool result = consumableItemData.Use(ctl);
            inventory.ConsumeItem(consumableItemData);
            return result;
        }

        public bool Load()
        {
            return true; 
        }

        public void Save()
        {
            // todo ES3の全体セーブに変更
            var jsonString = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(UserItemKey, jsonString);
            PlayerPrefs.Save();
        }

        public int AddItem(OwnedItemData ownedItemData, int amount = 1)
        {
            return inventory.AddItem(ownedItemData, amount);
        }
    }
}