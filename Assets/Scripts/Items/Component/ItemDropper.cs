using System.Collections.Generic;
using System.Linq;
using Character;
using Commons;
using Databrain;
using Databrain.Attributes;
using Databrain.Inventory;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items.Component
{
    public class ItemDropper : MonoBehaviour
    {
        // スロットのデータを取得
        private List<ItemMetaParameter> itemData;
        
        // private IEnumerable<KeyValuePair<int, float>> itemIndex;
        private GameObject[] prefabs;
        public LootTableData lootTable;
        private CharacterControl character;
        private bool isDropped;

        private void Start()
        {
            character = GetComponent<CharacterControl>();
            itemData = Resources.Load<ItemMetaData>("ItemMetaData").itemParameter;
            // itemIndex = itemData.Select((s, i) => KeyValuePair.Create(i, s.Weight));
            List<GameObject> loots = lootTable.DropLoot();
            prefabs = new GameObject[loots.Count];
            for (int i = 0; i < loots.Count; i++)
            {
                // todo Prefabを持つマネージャーを設置する。そのマネージャーから取ったPrefabにlootの情報を付与する。
                GameObject prefab = Instantiate(loots[i]);
                prefab.gameObject.SetActive(false);
                prefabs[i] = prefab;
            }
            /*prefabs = new GameObject[(int)itemData.Select(x => x.Weight).Sum()];
            int i = 0;
            foreach (ItemMetaParameter item in itemData)
            {
                for (int _ = 0; _ < item.Weight; _++)
                {
                    GameObject prefab = Instantiate(item.Prefab);
                    prefab.gameObject.SetActive(false);
                    prefabs[i] = prefab;
                    i++;
                }
            }*/
        }

        private void Update()
        {
            if (character.GetHealth() <= 0)
            {
                DropIfNeeded();
            }
        }

        private void DropIfNeeded()
        {
            if (isDropped || !character.GetIsDrop()) return;
            
            isDropped = true;
            // float dropChance = character.GetDropRate();
            
            // 素材をドロップする
            for (var i = 0; i < character.GetDropNum(); i++)
            { 
                // if (Random.Range(0, 1f) >= dropChance) continue;
                // int itemIdx = Tools.Lotto(itemIndex, character.GetKillerLuck() - character.GetLuck());
                GameObject prefab = prefabs[i];
                prefab.transform.rotation = transform.rotation;
                prefab.transform.position = new Vector3(transform.position.x + i, transform.position.y + 5, transform.position.z);
                prefab.gameObject.SetActive(true);
            }

            // 所持アイテムをドロップする
            foreach (var item in character.CharacterItems.inventorySlots)
            {
                float randomPosition = Random.Range(-1f, 1f);
                Vector3 position = new Vector3(transform.position.x + randomPosition, transform.position.y + 5, transform.position.z);
                GameObject prefab = Instantiate(item.item.itemPrefab, position, transform.rotation);
                prefab.AddComponent<ItemComponent>();
            }
            
            // todo 確率でキャラクター自体のドロップ
            // if (Random.Range(0, 1f) <= character.GetDropRate()) 
        }
    }
}