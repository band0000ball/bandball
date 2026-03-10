using Databrain;
using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using Items.ItemData;
using UnityEngine;

namespace Items
{
    public class ItemMetaData : DataObject
    {
        [SerializeField] public List<ItemMetaParameter> itemParameter = new();
        
        private const string OriginalItemsKey = "OriginalItemsMetaData";
        private static List<ItemMetaParameter> originalItemsMeta;

        private static List<ItemMetaParameter> OriginalItems =>
            originalItemsMeta ??= PlayerPrefs.HasKey(OriginalItemsKey) ? 
                JsonUtility.FromJson<List<ItemMetaParameter>>(PlayerPrefs.GetString(OriginalItemsKey)) 
                : null;
        
        public static ItemMetaParameter SearchItem(int itemId)
        {
            var hitItems = OriginalItems.Where(x => x.ID == itemId).ToList();
            if (hitItems.Count() > 1)
            {
                hitItems = hitItems.OrderByDescending(x => x.AddDateTime).ToList();
            }
            ItemMetaParameter itemMeta = hitItems.FirstOrDefault();
            if (itemMeta is null)
            {
                throw new NullReferenceException("アイテムのデータが見つかりませんでした。");
            }

            return itemMeta;
        }
    }
    
    [Serializable]
    public class ItemMetaParameter : IHaveWeight
    {
        // アイテムのメタデータ
        [SerializeField] private int id;
        [SerializeField] private string itemName;
        [SerializeField] private OwnedItemData.ItemType itemType;
        [SerializeField] private int itemId;
        [SerializeField] private int sellValue;
        [SerializeField] private float weight;
        [SerializeField] private DateTime addDateTime;
        [SerializeField] private GameObject prefab;
        
        public int ID => id;
        public float Weight => weight;
        public string ItemName => itemName;
        public OwnedItemData.ItemType ThisItemType => itemType;
        public int ThisItemID => itemId;
        public int SellValue => sellValue;
        public DateTime AddDateTime => addDateTime;
        public GameObject Prefab => prefab;

        public ItemMetaParameter(int id, string itemName, OwnedItemData.ItemType itemType, int itemId, int maxStack, float weight, 
            bool canMultiStack, DateTime expireDateTime, DateTime addDateTime, GameObject prefab)
        {
            this.id = id;
            this.itemName = itemName;
            this.itemType = itemType;
            this.itemId = itemId;
            this.weight = weight;
            this.addDateTime = addDateTime;
            this.prefab = prefab;
        }
    }
}