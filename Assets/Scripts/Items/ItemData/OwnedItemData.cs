using Databrain;
using System;
using System.Linq;
using Character;
using Databrain.Attributes;
using Databrain.Inventory;
using ItemSlot;
using Skill.SkillData;
using UnityEngine;
using UnityEngine.UIElements;

namespace Items.ItemData
{
    [Serializable]
    public class OwnedItemData : Databrain.Inventory.ItemData
    {
        public enum ItemType
        {
            Bag,
            Character,
            Energy,
            Food,
            Material,
            Shield,
            Tool,
            Valuable,
            Weapon,
        }

        // すべてのアイテムの基底クラス
        [SerializeField] private ItemType itemType;
        // [SerializeField] private int stackNumber = 1;
        [SerializeField] private float thickness = 1;
        [SerializeField] private DateTime _expireDateTime;
        
        public ItemType Type => itemType;
        public string ItemName => title;

        // public int StackNumber
        // {
        //     get => stackNumber;
        //     set => stackNumber = value;
        // }

        public float Thickness
        {
            get => thickness;
            set => thickness = value;
        }

        public DateTime ExpireDateTime
        {
            get => _expireDateTime;
            set => _expireDateTime = value;
        }
        
        public Sprite ItemSprite => icon;

        // todo Caller：ItemComponent, Listener：Inventoryとしてデータを登録する。 
        public virtual bool Use(CharacterControl ctl)
        {
            return false;
        }
    }

    [Serializable]
    public class HasUniqueIdData : OwnedItemData
    {
        [SerializeField] private string uniqueId;
        public string UniqueId => uniqueId;

        public void SetUniqueId() { uniqueId ??= Guid.NewGuid().ToString(); }
    }

    [Serializable]
    public class ExpendableItemData : HasUniqueIdData
    {
        // 装備可能なアイテムの基底クラス
        [DataObjectDropdown(true)]
        [SerializeField] private SkillData skill;
        [SerializeField] private float durability = 0;
        [SerializeField] private float maxDurability = 100f;
        [SerializeField] private int itemLevel = 1;
        [SerializeField] private int requiredLevel = 1;
        [SerializeField] private EnergySlot energyItem;
        [SerializeField] private int stackableEnergyItemNumber = 1;
        [SerializeField] private EnergyItemData.EnergyType settableEnergyType = EnergyItemData.EnergyType.None;
        
        public SkillData Skill => skill;
        public float Durability => durability;

        public void DecreaseDurability(float decreaseValue)
        {
            durability -= decreaseValue;
            if (durability < 0) durability = 0;
        }
        
        public void FixItem(int fixValue)
        {
            durability += fixValue;
            if (durability > maxDurability) durability = maxDurability;
        }

        private int[] SearchItemIndex(InventoryData inventory, OwnedItemData item)
        {
            return inventory.inventorySlots
                .Where(x => x.item.initialGuid == item.initialGuid)
                .OrderBy(x => x.amount)
                .Select((_, i) => i).ToArray();
        }

        private int RemoveItemFromInventory(InventoryData inventory, EnergyItemData energy, int settableNum)
        {
            if (settableNum <= 0) return 0;
            int[] amounts = SearchItemIndex(inventory, energy).Select(x => inventory.inventorySlots[x].amount).ToArray();
                
            int returnNum;
            if (settableNum <= amounts.Sum())
            {
                returnNum = settableNum;
                foreach (var num in amounts)
                {
                    int useNum = settableNum > num ? num : settableNum;
                    var notRemoved = inventory.RemoveItem(energy, useNum);
                    if (notRemoved > 0)
                    {
                        // todo 異常なのでログを出す
                    }
                    settableNum -= num;
                    if (settableNum < 0) break;
                }
            }
            else
            {
                returnNum = amounts.Sum();
                foreach (var num in amounts)
                {
                    var notRemoved = inventory.RemoveItem(energy, num);
                    if (notRemoved > 0)
                    {
                        // todo 異常なのでログを出す
                    }
                }
            }
            return returnNum;
        }

        public int ReloadEnergy(InventoryData inventory)
        {
            int reloadableNum = stackableEnergyItemNumber - energyItem.amount;
            if (reloadableNum < 0) return 0;
            
            // インベントリからセットしているデータを取得し、リロード可能数の計算を行う。
            int setNum = RemoveItemFromInventory(inventory, energyItem.item, reloadableNum);
            
            // todo setNumが0でない場合はリロード時間を挟む処理をMonobehaviour側で行う。
            // energyItem.amount += setNum;
            return setNum;
        }

        public int SetEnergy(EnergyItemData energy, InventoryData inventory)
        {
            // セットしたいアイテムの数をセットする分だけ減少させ、セット可能な数を返す
            // インベントリから指定のアイテムを取得し、複数のスタックをまとめて扱う
            if (energy.energyType != settableEnergyType)
            {
                // todo タイプが違うためSetできなかったPopupを出す。
                return 0;
            }
            int setNum;
            
            if (energyItem.initialGuid == energy.initialGuid)
            {
                setNum = ReloadEnergy(inventory);
            }
            else
            {
                if (energyItem is not null)
                {
                    var restAmount = inventory.AddItem(energyItem.item, energyItem.amount);
                    if (restAmount > 0)
                    {
                        // todo AddNumできなかったときエラーのPopupを出す
                    }
                }
                energyItem = energy.runtimeClone as EnergySlot;
                setNum = RemoveItemFromInventory(inventory, energy, stackableEnergyItemNumber);
                
                // todo setNumが0でない場合はロード時間を挟む処理をMonobehaviour側で行う。
                // energyItem!.ResetAmount(setNum);
            }

            return setNum;
        }
    }
}