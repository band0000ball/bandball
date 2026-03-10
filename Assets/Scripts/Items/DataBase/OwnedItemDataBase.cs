using System.Collections.Generic;
using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    public abstract class OwnedItemDataBase<T> : ScriptableObject where T : OwnedItemData
    {
        [SerializeField]
        private List<T> itemList = new();
        
        public List<T> ItemList => itemList;
    }
}