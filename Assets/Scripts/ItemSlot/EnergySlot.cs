using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Databrain.Attributes;
using Databrain.Inventory;
using Items.ItemData;
using UnityEngine;
using InventorySlotData = Databrain.Inventory.InventorySlotData;

namespace ItemSlot
{
    public class EnergySlot : InventoryData
    {
        public EnergyItemData item => (EnergyItemData)inventorySlots[0].item;

        public int amount {
            get => inventorySlots[0].amount; 
            set => inventorySlots[0].amount = value; 
        }

        public IEnumerator AddAmount(int addAmount)
        {
            yield return new WaitForSeconds(item.reloadTime);
            amount += addAmount;
        }

        public IEnumerator ResetAmount(int resetAmount)
        {
            yield return new WaitForSeconds(item.reloadTime);
            amount = resetAmount;
        }
    }
}