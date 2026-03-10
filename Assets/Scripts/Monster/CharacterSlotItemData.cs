using Databrain;
using System;
using System.Collections.Generic;
using System.Linq;
using Databrain.Inventory;
using Items.ItemData;
using UnityEngine;

namespace Monster
{
    [Serializable]
    public class CharacterSlotItemData : InventoryData
    {
        // [SerializeField] public ItemWithSkillData[] slotItemData = new ItemWithSkillData[10];
        [SerializeField] public bool[] isChanged = new bool[10];
        public new InventoryType inventoryType = InventoryType.equipment;
        
        public void InitByCharacterSlot(DataLibrary dl, int characterID, int level)
        {
            inventorySlots = new List<InventorySlotData>();
            for (int i = 0; i < 10; i++) { inventorySlots.Add(null); }
            var newSlotItemData = dl.GetAllInitialDataObjectsByType<CharacterSlot>().Select(x => x)
                .FirstOrDefault(x => x.characterId == characterID && x.characterLevel == level)
                ?.slotItemData;
            if (newSlotItemData is null) return;
            for (int i = 0; i < newSlotItemData.Length; i++)
            {
                inventorySlots[i] = new InventorySlotData(this, newSlotItemData[i], 1);
            }
            // inventorySlots = newSlotItemData.Select(x => !string.IsNullOrEmpty(x) ? (ItemWithSkillData)dl.GetInitialDataObjectByGuid(x) : ScriptableObject.CreateInstance<ItemWithSkillData>()).ToArray();
            // slotItemData = newSlotItemData.Select(x => !string.IsNullOrEmpty(x) ? (ItemWithSkillData)dl.GetInitialDataObjectByGuid(x) : ScriptableObject.CreateInstance<ItemWithSkillData>()).ToArray();
        }

        public static bool[] GetChooseAttackItems(bool[] activate, bool[] attackItem)
        {
            return activate.Zip(attackItem, (a, b) => a & b).ToArray();
        }

        public float GetShieldValues() { return inventorySlots.Select(x => x.item is ShieldItemData shieldItem ? shieldItem.shield : 0).Sum(); }

        public void DecreaseShield(float decreaseValue)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot.item is not ShieldItemData shieldItem) continue;
                shieldItem.DecreaseDurability(decreaseValue / shieldItem.shield);
                if (shieldItem.Durability <= 0) decreaseValue -= shieldItem.shield;
                if (decreaseValue <= 0) return;
            }
        }

        public (int remainingNum, bool[] index, bool inBag) AddItem(OwnedItemData itemData, int amount)
        {
            // inBagがtrueの時は使用しない
            bool[] setIndex = new bool[10];
            int remainingNum = amount;
            bool isInBag = false;
            
            if (itemData is HasUniqueIdData data)
            {
                data.SetUniqueId();
                if (data is ShieldItemData or BagItemData or WeaponItemData)
                {
                    remainingNum = base.AddItem(data, remainingNum);
                    if (remainingNum == 0) return (remainingNum, setIndex, false);
                }
            }
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].item is not BagItemData bagItem) continue;
                
                remainingNum = bagItem.AddItem(itemData, remainingNum);
                isInBag = remainingNum <= 0;
                if (!isInBag) continue;
                
                setIndex[i] = true;
                if (remainingNum <= 0) return (remainingNum, setIndex, true);
            }
            
            remainingNum = base.AddItem(itemData, remainingNum);
            return (remainingNum, setIndex, isInBag);
        }
        
        /*public (int index, bool inBag) AddItem(OwnedItemData itemData, OwnedItemData itemDataModel, int amount)
        {
            int emptyIndex = FirstEmptySlot();
            if (emptyIndex >= slotItemData.Length) emptyIndex = -1;
            if (itemData is BagItemData or ShieldItemData or WeaponItemData && emptyIndex >= 0)
            {
                // アイテムをスロットにセットする。
                slotItemData[emptyIndex] = (ItemWithSkillData)itemData;
                return (emptyIndex, false);
            }
            
            foreach (var slot in slotItemData.Select((x, i) => new { x, i }))
            {
                // バッグがあった場合はバッグにアイテムを追加する。
                if (slot.x is not BagItemData bagItemData) continue;
                if (!bagItemData.AddItem(itemData, itemDataModel, amount)) continue;
                isChanged[slot.i] = true;
                return (slot.i, true);
            }

            if (emptyIndex < 0) return (-1, false);
            // アイテムをスロットにセットする。
            slotItemData[emptyIndex] = (ItemWithSkillData)itemData;
            return (emptyIndex, true);
        }*/

        public void Save()
        {
            if (isChanged.Any(x => x == false)) return;
            foreach (var changed in isChanged.Select((x, i) => new { x, i }))
            {
                if (!changed.x) continue;
                if (inventorySlots[changed.i].item is not BagItemData bagItemData) continue;
                bagItemData.Save();
            }
            isChanged = new bool[10];
        }
        
        public Sprite[] SlotItemSprites => inventorySlots.Select(x => x.item.icon).ToArray();

        public OwnedItemData[] Items()
        {
            List<OwnedItemData> items = new List<OwnedItemData>();
            foreach (var slot in inventorySlots)
            {
                if (slot is null) continue;
                
                items.Add(slot.item as OwnedItemData);
                if (slot.item is BagItemData bagItemData)
                {
                    items.AddRange(bagItemData.inventory.inventorySlots.Select(x => x.item as OwnedItemData));
                }
            }
            return items.ToArray();
        }

        private int FirstEmptySlot()
        {
            foreach (var slot in inventorySlots.Select((x, i) => new { x, i }))
            {
                OwnedItemData slotItem = slot.x.item as OwnedItemData;
                if (slotItem?.ItemSprite is null) return slot.i;
            }

            return -1;
        }
    }
}