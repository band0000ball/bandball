using Character;
using Databrain.Inventory;
using DG.Tweening;
using Items.DataStore;
using Items.ItemData;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items.Component
{
    [RequireComponent(typeof(Collider))]   
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(WorldItemObjectData))]
    public class ItemComponent : MonoBehaviour
    {
        // フィールドにポップするアイテムにつけるクラス
        [SerializeField] private string itemGuid;
        [SerializeField] private OwnedItemData.ItemType itemType;
        private Collider _colliderCache;
        private Rigidbody _rigidbodyCache;
        private OwnedItemData _itemDataCache;
        private AmmoItemDataStore _ammoItemDataStore;
        private BagItemDataStore _bagItemDataStore;
        private CharacterItemDataStore _characterItemDataStore;
        private FoodItemDataStore _foodItemDataStore;
        private MaterialItemDataStore _materialItemDataStore;
        private ShieldItemDataStore _shieldItemDataStore;
        private ToolItemDataStore _toolItemDataStore;
        private ValuableItemDataStore _valuableItemDataStore;
        private WeaponItemDataStore _weaponItemDataStore;
        private int _itemAmount;
        private const float JumpForce = 0.5f;

        private void Awake()
        {
            _colliderCache = GetComponent<Collider>();
            _rigidbodyCache = GetComponent<Rigidbody>();
            _ammoItemDataStore = FindAnyObjectByType<AmmoItemDataStore>();
            _bagItemDataStore = FindAnyObjectByType<BagItemDataStore>();
            _characterItemDataStore = FindAnyObjectByType<CharacterItemDataStore>();
            _foodItemDataStore = FindAnyObjectByType<FoodItemDataStore>();
            _materialItemDataStore = FindAnyObjectByType<MaterialItemDataStore>();
            _shieldItemDataStore = FindAnyObjectByType<ShieldItemDataStore>();
            _toolItemDataStore = FindAnyObjectByType<ToolItemDataStore>();
            _valuableItemDataStore = FindAnyObjectByType<ValuableItemDataStore>();
            _weaponItemDataStore = FindAnyObjectByType<WeaponItemDataStore>();
            _itemDataCache = itemType switch
            {
                OwnedItemData.ItemType.Energy => _ammoItemDataStore.FindWithID(itemGuid),
                OwnedItemData.ItemType.Bag => _bagItemDataStore.FindWithID(itemGuid),
                OwnedItemData.ItemType.Character => _characterItemDataStore.FindWithID(itemGuid),
                OwnedItemData.ItemType.Food => _foodItemDataStore.FindWithID(itemGuid),
                OwnedItemData.ItemType.Material => _materialItemDataStore.FindWithID(itemGuid),
                OwnedItemData.ItemType.Shield => _shieldItemDataStore.FindWithID(itemGuid),
                OwnedItemData.ItemType.Valuable => _valuableItemDataStore.FindWithID(itemGuid),
                OwnedItemData.ItemType.Tool => _toolItemDataStore.FindWithID(itemGuid),
                OwnedItemData.ItemType.Weapon => _weaponItemDataStore.FindWithID(itemGuid),
                _ => _itemDataCache
            };
            _itemAmount = 1;
        }

        private void OnEnable()
        {
            _colliderCache.enabled = false;
            
            var transformCache = transform;
            var dropPosition = transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 0f, Random.Range(-1.0f, 1.0f));
            transformCache.DOLocalMove(dropPosition, 0.5f);
            var defaultScale = transformCache.localScale;
            transformCache.localScale = Vector3.zero;
            transformCache.DOScale(defaultScale, 0.5f)
                .SetEase(Ease.OutBounce)
                .OnComplete(() => { _colliderCache.enabled = true; });
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) _rigidbodyCache.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterControl character = other.GetComponent<CharacterControl>();
            if (!character) return;
            if (character.GetHealth() <= 0) return;
            if (character.Input is not PlayerInput) return;
            
            var added = character.AddItem(_itemDataCache, _itemAmount);
            if (added.remain > 0)
            {
                _itemAmount = added.remain;
                return;
            }
            
            if (!added.result) Debug.Log("アイテムが正常に取得できませんでした。");
            
            /*OwnedItemData itemDataModel;
            OwnedItemData findItem;
            switch (_itemDataCache.Type)
            {
                case OwnedItemData.ItemType.Ammo:
                    findItem = _ammoItemDataStore.FindWithID(_itemDataCache.ItemId);
                    itemDataModel = ScriptableObject.CreateInstance<AmmoItemData>();
                    break;
                case OwnedItemData.ItemType.Bag:
                    findItem = _bagItemDataStore.FindWithID(_itemDataCache.ItemId);
                    itemDataModel = ScriptableObject.CreateInstance<BagItemData>();
                    break;
                case OwnedItemData.ItemType.Character:
                    findItem = _characterItemDataStore.FindWithID(_itemDataCache.ItemId);
                    itemDataModel = ScriptableObject.CreateInstance<CharacterItemData>();
                    break;
                case OwnedItemData.ItemType.Food:
                    findItem = _foodItemDataStore.FindWithID(_itemDataCache.ItemId);
                    itemDataModel = ScriptableObject.CreateInstance<FoodItemData>();
                    break;
                case OwnedItemData.ItemType.Material:
                    findItem = _materialItemDataStore.FindWithID(_itemDataCache.ItemId);
                    itemDataModel = ScriptableObject.CreateInstance<MaterialItemData>();
                    break;
                case OwnedItemData.ItemType.Shield:
                    findItem = _shieldItemDataStore.FindWithID(_itemDataCache.ItemId);
                    itemDataModel = ScriptableObject.CreateInstance<ShieldItemData>();
                    break;
                case OwnedItemData.ItemType.Tool:
                    findItem = _toolItemDataStore.FindWithID(_itemDataCache.ItemId);
                    itemDataModel = ScriptableObject.CreateInstance<ToolItemData>();
                    break;
                case OwnedItemData.ItemType.Valuable:
                    findItem = _valuableItemDataStore.FindWithID(_itemDataCache.ItemId);
                    itemDataModel = ScriptableObject.CreateInstance<ValuableItemData>();
                    break;
                case OwnedItemData.ItemType.Weapon:
                    findItem = _weaponItemDataStore.FindWithID(_itemDataCache.ItemId);
                    itemDataModel = ScriptableObject.CreateInstance<WeaponItemData>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            itemDataModel.CopyFrom(findItem);
            if (character.AddItem(_itemDataCache, itemDataModel, _itemNum))
            {
                character.SaveItem();
                gameObject.SetActive(false);
            }*/

            character.SaveItem();
            gameObject.SetActive(false);
            foreach (var ownedItem in character.CharacterItems.Items())
            {
                if (ownedItem.ItemName != "") Debug.Log(ownedItem.ItemName + "を所持");
            }
        }
    }
}