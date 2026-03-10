using System;
using Commons;
using Items.ItemData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ProfileItemIconButton : MonoBehaviour
    {
        private bool _isSelected;
        private int _index;
        private Image _image;
        private TMP_Text _durability;
        private Image _background;
        private Image _border;

        private void Awake()
        {
            _isSelected = false;
            _index = int.Parse(transform.Find("Index").GetComponentInChildren<TMP_Text>().text);
            _image = transform.Find("Icon").GetComponent<Image>();
            _durability = transform.Find("Durability").GetComponentInChildren<TMP_Text>();
        }

        private void Update()
        {
            // if (_isSelected) 
        }

        public int GetIndex() { return _index; }

        public void Activate() { _isSelected = true; }
        public void Deactivate() { _isSelected = false; }
        public void Set(ExpendableItemData item)
        {
            _image.sprite = item.icon;
            _durability.text = ((int)item.Durability).ToString();
            _background.color = item.rarity.color;
            // todo エフェクトも入れたい
            _border.color = item switch
            {
                WeaponItemData weapon => AttributeMagnification.AttColor[(AttributeMagnification.Attribute)weapon.attribute],
                ShieldItemData shield => AttributeMagnification.AttColor[(AttributeMagnification.Attribute)shield.attribute],
                _ => Color.gray
            };
        }
    }
}