using System.Collections.Generic;
using Commons;
using DG.Tweening;
using static Commons.GameBalance;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Character
{
    /// <summary>
    /// キャラクターのUI表示を担当するコンポーネント
    /// HPバー、スタミナバー、属性アイコン等の表示管理
    /// </summary>
    [RequireComponent(typeof(CharacterControl))]
    public class CharacterStatusUI : MonoBehaviour
    {
        #region Private Fields

        private CharacterControl _control;
        private CharacterCombat _combat;
        private bool _isPlayer;

        // HP/Stamina UI
        private Slider _hpBar;
        private Slider _staminaBar;
        private Slider _extraHpBar;
        private TMP_Text _hpText;
        private TMP_Text _extraHpNum;
        private Canvas _hpBarCanvas;
        private Transform _extraHpBarCanvas;
        private List<Slider> _extraHpSliders;

        // Attribute UI
        private Dictionary<AttributeMagnification.Attribute, GameObject> _attributeIcons;
        private AttributeMagnification.Attribute _activeAttribute;

        // Guard UI
        private Image[] _guardIcons;
        private Tweener[] _guardIconTweenerArray;

        // Item UI
        private UI.BottomItemIconButton[] _itemIcons;

        // Cached values
        private int _maxHealth;
        private int _maxStamina;

        #endregion

        #region Public Properties

        public UI.BottomItemIconButton[] ItemIcons => _itemIcons;

        #endregion

        #region Initialization

        public void Initialize(CharacterControl control, CharacterCombat combat, Canvas userUI, Canvas hpBarCanvas,
            int maxHealth, int maxStamina, int guardNum, int level, bool isPlayer)
        {
            _control = control;
            _combat = combat;
            _hpBarCanvas = hpBarCanvas;
            _maxHealth = maxHealth;
            _maxStamina = maxStamina;
            _isPlayer = isPlayer;
            _extraHpSliders = new List<Slider>();

            if (_hpBarCanvas == null) return;

            InitializeSliders();
            InitializeLevelDisplay(level);

            if (isPlayer && userUI != null)
            {
                InitializePlayerUI(userUI, guardNum);
            }

            // イベント購読
            if (_combat != null)
            {
                _combat.OnGuardBroken += OnGuardBroken;
                _combat.OnGuardRecovered += OnGuardRecovered;
            }
        }

        private void InitializeSliders()
        {
            foreach (var component in _hpBarCanvas.GetComponentsInChildren(typeof(Slider)))
            {
                Slider slider = (Slider)component;
                switch (slider.name)
                {
                    case "HPSlider":
                        _hpBar = slider;
                        _hpBar.maxValue = _maxHealth;
                        _hpBar.value = _maxHealth;
                        break;
                    case "StaminaSlider":
                        _staminaBar = slider;
                        _staminaBar.maxValue = _maxStamina;
                        _staminaBar.value = _maxStamina;
                        break;
                    case "ExtraHPSlider":
                        _extraHpBar = slider;
                        _extraHpBar.maxValue = _maxHealth;
                        _extraHpBar.value = _maxHealth;
                        break;
                }
            }

            _hpText = _hpBarCanvas.transform.Find("HPNumber")?.GetComponent<TMP_Text>();
            _hpText?.SetText(FormatHp(_maxHealth, _maxHealth));

            _extraHpBarCanvas = _hpBarCanvas.transform.Find("ExtraHP");
            if (_extraHpBarCanvas != null)
            {
                _extraHpNum = _extraHpBarCanvas.Find("ExtraHPNum")?.Find("Number")?.GetComponent<TMP_Text>();
                if (_extraHpNum != null)
                {
                    _extraHpNum.text = "0";
                }
                _extraHpBarCanvas.gameObject.SetActive(false);
            }
        }

        private void InitializeLevelDisplay(int level)
        {
            var levelBg = _hpBarCanvas.transform.Find("LevelBackGround");
            if (levelBg != null)
            {
                TMP_Text lvNum = levelBg.Find("LvNum")?.GetComponent<TMP_Text>();
                lvNum?.SetText(level.ToString());
            }
        }

        private void InitializePlayerUI(Canvas userUI, int guardNum)
        {
            // 属性アイコン初期化
            var profileButton = _hpBarCanvas.transform.Find("Profile Button");
            if (profileButton != null)
            {
                var attIconCanvas = profileButton.Find("AttributeIcon");
                if (attIconCanvas != null)
                {
                    _attributeIcons = new Dictionary<AttributeMagnification.Attribute, GameObject>
                    {
                        [AttributeMagnification.Attribute.Frame] = attIconCanvas.Find("FramePrefab")?.gameObject,
                        [AttributeMagnification.Attribute.Aqua] = attIconCanvas.Find("AquaPrefab")?.gameObject,
                        [AttributeMagnification.Attribute.Electric] = attIconCanvas.Find("ElectricPrefab")?.gameObject,
                        [AttributeMagnification.Attribute.Plant] = attIconCanvas.Find("PlantPrefab")?.gameObject,
                        [AttributeMagnification.Attribute.Ground] = attIconCanvas.Find("GroundPrefab")?.gameObject,
                        [AttributeMagnification.Attribute.Ice] = attIconCanvas.Find("IcePrefab")?.gameObject,
                        [AttributeMagnification.Attribute.Oil] = attIconCanvas.Find("OilPrefab")?.gameObject,
                        [AttributeMagnification.Attribute.Wind] = attIconCanvas.Find("WindPrefab")?.gameObject,
                        [AttributeMagnification.Attribute.Toxin] = attIconCanvas.Find("ToxinPrefab")?.gameObject,
                        [AttributeMagnification.Attribute.Spirit] = attIconCanvas.Find("SpiritPrefab")?.gameObject
                    };
                }
            }
            _activeAttribute = AttributeMagnification.Attribute.None;

            // アイテムアイコン初期化
            _itemIcons = userUI.GetComponentsInChildren<UI.BottomItemIconButton>();
            foreach (var itemIcon in _itemIcons)
            {
                itemIcon.SetCtl(_control);
            }

            // ガードアイコン初期化
            InitializeGuardIcons(guardNum);
        }

        private void InitializeGuardIcons(int guardNum)
        {
            _guardIconTweenerArray = new Tweener[guardNum];
            _guardIcons = new Image[guardNum];

            var shieldArea = _hpBarCanvas.transform.Find("ShieldArea")?.GetComponent<RectTransform>();
            if (shieldArea == null) return;

            Image shieldImg = shieldArea.gameObject.GetComponentInChildren<Image>();
            if (shieldImg == null) return;

            _guardIcons[0] = shieldImg;
            for (int i = 0; i < guardNum - 1; i++)
            {
                _guardIcons[i + 1] = Object.Instantiate(shieldImg, shieldArea.transform, false);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// UI更新（Updateから呼び出し）
        /// </summary>
        public void UpdateUI(float currentHealth, float currentStamina)
        {
            if (_hpBar != null)
            {
                _hpBar.value = currentHealth;
            }

            if (_staminaBar != null)
            {
                _staminaBar.value = currentStamina;
            }

            _hpText?.SetText(FormatHp((int)Tools.RoundValue(currentHealth), _maxHealth));
        }

        /// <summary>
        /// 属性アイコン更新
        /// </summary>
        public void UpdateAttribute(float[] attributes)
        {
            if (!_isPlayer || _attributeIcons == null) return;

            var maxAtt = (AttributeMagnification.Attribute)Tools.MaxAndArg(attributes).arg;

            if (_activeAttribute == maxAtt) return;

            if (_activeAttribute != AttributeMagnification.Attribute.None &&
                _attributeIcons.TryGetValue(_activeAttribute, out var prevIcon) && prevIcon)
            {
                prevIcon.SetActive(false);
            }

            if (_attributeIcons.TryGetValue(maxAtt, out var newIcon) && newIcon)
            {
                newIcon.SetActive(true);
            }

            _activeAttribute = maxAtt;
        }

        /// <summary>
        /// アイテムアイコン設定
        /// </summary>
        public void SetItemIcon(int index, Sprite icon)
        {
            if (_itemIcons == null || index < 0 || index >= _itemIcons.Length) return;
            _itemIcons[index]?.Set(icon);
        }

        /// <summary>
        /// アイテムクールダウン表示
        /// </summary>
        public void ActivateItemCooldown(int index, float time)
        {
            if (_itemIcons == null || index < 0 || index >= _itemIcons.Length) return;
            _itemIcons[index]?.ActivateTimeIcon(time);
        }

        /// <summary>
        /// スキルアクティブ状態設定
        /// </summary>
        public void SetSkillActive(int index, Skill.SkillData.SkillData skill)
        {
            if (_itemIcons == null || index < 0 || index >= _itemIcons.Length) return;
            _itemIcons[index]?.Activate(skill);
        }

        /// <summary>
        /// スキル非アクティブ状態設定
        /// </summary>
        public void SetSkillDeactive(int index)
        {
            if (_itemIcons == null || index < 0 || index >= _itemIcons.Length) return;
            _itemIcons[index]?.Deactivate();
        }

        /// <summary>
        /// 追加HP表示の更新
        /// </summary>
        public void UpdateExtraHealth(List<float> extraHealths)
        {
            if (!_extraHpBarCanvas) return;

            // スライダー数の調整
            while (_extraHpSliders.Count < extraHealths.Count)
            {
                var extraHpBar = Object.Instantiate(_extraHpBar, _hpBar.transform, false);
                extraHpBar.GetComponent<SortingGroup>().sortingOrder = _extraHpSliders.Count + 1;
                extraHpBar.maxValue = _maxHealth;
                _extraHpSliders.Add(extraHpBar);
            }

            while (_extraHpSliders.Count > extraHealths.Count)
            {
                var last = _extraHpSliders[^1];
                _extraHpSliders.RemoveAt(_extraHpSliders.Count - 1);
                Object.Destroy(last.gameObject);
            }

            // 値の更新
            for (int i = 0; i < extraHealths.Count; i++)
            {
                _extraHpSliders[i].value = extraHealths[i];
            }

            // 表示切替
            _extraHpBarCanvas.gameObject.SetActive(extraHealths.Count > 0);
            if (_extraHpNum)
            {
                _extraHpNum.text = extraHealths.Count.ToString();
            }
        }

        #endregion

        #region Event Handlers

        private void OnGuardBroken(int index)
        {
            if (_guardIcons == null || index < 0 || index >= _guardIcons.Length) return;
            _guardIcons[index].gameObject.SetActive(false);
        }

        private void OnGuardRecovered(int index)
        {
            if (_guardIcons == null || index < 0 || index >= _guardIcons.Length) return;

            _guardIcons[index].gameObject.SetActive(true);

            // 回復アニメーション
            if (_guardIconTweenerArray[index] != null)
            {
                _guardIconTweenerArray[index].Kill();
                _guardIconTweenerArray[index] = null;
                _guardIcons[index].transform.localScale = Vector3.one;
            }

            _guardIconTweenerArray[index] = _guardIcons[index].transform.DOPunchScale(
                punch: Vector3.one * GUARD_ICON_PUNCH_SCALE,
                duration: GUARD_ICON_ANIMATION_DURATION,
                vibrato: 1
            ).SetEase(Ease.OutExpo);
        }

        #endregion

        #region Private Methods

        private string FormatHp(int currentHp, int maxHp)
        {
            return $"{currentHp} / {maxHp}";
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (_combat != null)
            {
                _combat.OnGuardBroken -= OnGuardBroken;
                _combat.OnGuardRecovered -= OnGuardRecovered;
            }

            // Tweenerのクリーンアップ
            if (_guardIconTweenerArray != null)
            {
                foreach (var tweener in _guardIconTweenerArray)
                {
                    tweener?.Kill();
                }
            }
        }

        #endregion
    }
}
