using System;
using Character;
using Items.ItemData;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Profile : MonoBehaviour
    {
        [SerializeField] private RectTransform menuCanvas;
        [SerializeField] private RectTransform attributeImage;
        [SerializeField] private RectTransform characterStatusCanvas;
        [SerializeField] private RectTransform itemCanvas;
        [SerializeField] private RectTransform skillCanvas;
        [SerializeField] private RectTransform setItemCanvas;
        [SerializeField] private GameObject itemStatusPrefab;
        [SerializeField] private GameObject inventorySlotsPrefab;
        [SerializeField] private GameObject itemSlotPrefab;
        private ProfileItemIconButton[] _profileItemIconButtons;
        private CharacterControl _character;

        private int _selectedItem;

        private void Awake()
        {
            menuCanvas = GetComponent<RectTransform>();
            _selectedItem = -1;
        }

        void Start()
        {
            _profileItemIconButtons = menuCanvas.GetComponentsInChildren<ProfileItemIconButton>();
        }

        public void Init(CharacterControl character)
        {
            _character = character;
            for (int i = 0; i < _profileItemIconButtons.Length; i++)
            {
                _profileItemIconButtons[i].Set(character.CharacterItems.inventorySlots[i].item as ExpendableItemData);
            }
        }

        public void Clicked(int selectedItem)
        {
            if (selectedItem == _selectedItem)
            {
                _selectedItem = -1;
                InitialView();
            }
            else
            {
                // todo バッグはインベントリ、盾と武器はステータス、他のアイテムは説明欄のみを表示する
                ExpendableItemData item = _character.CharacterItems.inventorySlots[selectedItem].item as ExpendableItemData;
                if (item is WeaponItemData weapon)
                {
                    // todo ステータスを表示
                }
                else if (item is ShieldItemData shield)
                {
                    // todo ステータスを表示
                }
                else if (item is BagItemData bag)
                {
                    RectTransform itemSlots = itemSlotPrefab.transform.Find("Content").GetComponent<RectTransform>();
                    foreach (GameObject child in itemSlots) { Destroy(child); }
                    
                    for (int i = 0; i < bag.inventory.inventorySlots.Count; i++)
                    {
                        Instantiate(itemSlotPrefab, itemSlots.transform);
                    }
                    
                    // todo インベントリ内容を表示
                }
                else
                {
                    // todo 説明文を表示
                }
            }
            _selectedItem = selectedItem;
        }

        private void InitialView()
        {
            // todo 初期状態は属性値のグラフを表示
        }
    }
}