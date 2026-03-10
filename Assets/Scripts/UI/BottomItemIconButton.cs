using System;
using System.Linq;
using Character;
using Ricimi;
using Skill.SkillData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(CleanButton))]
    public class BottomItemIconButton : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        [SerializeField] private Sprite defaultIcon;
        private CleanButton _cleanButton;
        private Image _icon;
        private int _number;
        private GameObject _shieldUseEffect;
        private GameObject _bagUseEffect;
        private GameObject _weaponUseEffect;
        private GameObject _timeIcon;
        private Animator _animator;
        private TMP_Text _timeText;
        private CharacterControl _ctl;

        private void Awake()
        {
            _cleanButton = transform.GetComponent<CleanButton>();
            _icon = GetComponentsInChildren<Image>().Select(x => x).First(x => x.name == "Icon");
            _icon.color = new Color(255, 255, 255, 255);
            _number = int.Parse(GetComponentInChildren<TMP_Text>().text) - 1;
            if (_number < 0) _number = 9;
            _shieldUseEffect = transform.Find("ShieldUseIcon").gameObject;
            _bagUseEffect = transform.Find("BagUseIcon").gameObject;
            _weaponUseEffect = transform.Find("WeaponUseIcon").gameObject;
            _timeIcon = transform.Find("TimeIcon").gameObject;
            _animator = _timeIcon.GetComponent<Animator>();
            _timeText = _timeIcon.GetComponentInChildren<TMP_Text>();

            Set();
            Deactivate();
        }

        private void FixedUpdate()
        {
            float cooldown = _ctl.GetFireCooldown(_number);
            if (!_timeIcon.activeSelf && cooldown > 0) return;
            _timeText.text = cooldown < 10 ? $"{cooldown:0.0}" : $"{cooldown:#}";
            if (cooldown <= 0) _timeIcon.SetActive(false);
        }

        public void ActivateTimeIcon(float cooldown)
        {
            if (_timeIcon.activeSelf) return;
            _timeIcon.SetActive(true);
            _animator.SetFloat(Speed, 1 / cooldown);
            _timeText.text = cooldown < 10 ? $"{cooldown:0.0}" : $"{cooldown:#}";
        }

        public void SetCtl(CharacterControl ctl) => _ctl = ctl;

        public bool Set(Sprite slotItem = null)
        {
            void ResetSize(float size)
            {
                _icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                _icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }

            if (slotItem is null)
            {
                _cleanButton.canvasGroup.interactable = false;
                _icon.sprite = defaultIcon;
                // _icon.color = new Color(83, 101, 125, 255);
                ResetSize(100);
                return false;
            }

            _cleanButton.canvasGroup.interactable = true;
            _cleanButton.onClick.AddListener(OnClick);
            _icon.sprite = slotItem;
            ResetSize(120);
            return true;
        }

        public void Activate(SkillData skill)
        {
            switch (skill)
            {
                case ShieldSkillData:
                    _shieldUseEffect.SetActive(true);
                    break;
                case BuffSkillData:
                    _bagUseEffect.SetActive(true);
                    break;
                case AttackSkillData:
                    _weaponUseEffect.SetActive(true);
                    break;
                default:
                    Deactivate();
                    break;
            }
            // _cleanButton.canvasGroup.alpha = 1;
            // _icon.color = new Color(255, 255, 255, 255);
        }

        public void Deactivate()
        {
            // _cleanButton.canvasGroup.alpha = 0.5f;
            // _icon.color = new Color(83, 101, 125, 255);
            if (_weaponUseEffect.activeSelf) _weaponUseEffect.SetActive(false);
            if (_shieldUseEffect.activeSelf) _shieldUseEffect.SetActive(false);
            if (_bagUseEffect.activeSelf) _bagUseEffect.SetActive(false);
        }

        private void OnClick()
        {
            _ctl.SwapSkillActive(_number);
        }
    }
}
