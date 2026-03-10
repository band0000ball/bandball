using System.Collections.Generic;
using Databrain;
using Items.DataBase;
using Items.ItemData;
using UnityEngine;

namespace Items.DataStore
{
    public abstract class OwnedItemDataStore<T> : MonoBehaviour where T : OwnedItemData
    {
        public DataLibrary data;
        
        protected List<T> dataBase => data.GetAllInitialDataObjectsByType<T>();
        public List<T> DataBase => dataBase;
        
        protected const string OriginalItemsKey = "OriginalItemsKey";

        public T FindWithName(string itemName)
        {
            return string.IsNullOrEmpty(itemName) ? null : dataBase.Find(x => x.ItemName == itemName);
        }

        public T FindWithID(string id)
        {
            return dataBase.Find(x => x.initialGuid == id);
        }
    }
}