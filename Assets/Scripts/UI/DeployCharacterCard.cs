using System.Collections.Generic;
using Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 出撃画面の各キャラクターカード。
    /// DeployScreen が Instantiate して Initialize() を呼ぶ。
    /// </summary>
    public class DeployCharacterCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _slotSizeText;
        [SerializeField] private TextMeshProUGUI _stateText;     // 「操作」「パートナー」「-」

        [SerializeField] private Button _setOperatorButton;
        [SerializeField] private Button _togglePartnerButton;

        [SerializeField] private GameObject _operatorHighlight;  // 操作キャラ選択時に表示
        [SerializeField] private GameObject _partnerHighlight;   // パートナー選択時に表示

        private OwnedCharacterData _data;
        private DeployScreen _screen;

        // ── 初期化 ──────────────────────────────────────────────────────────

        public void Initialize(OwnedCharacterData data, DeployScreen screen)
        {
            _data   = data;
            _screen = screen;

            if (_setOperatorButton != null)
                _setOperatorButton.onClick.AddListener(() => _screen.SetOperator(_data));
            if (_togglePartnerButton != null)
                _togglePartnerButton.onClick.AddListener(() => _screen.TogglePartner(_data));

            UpdateStaticFields();
        }

        /// <summary>選択状態が変わったときに DeployScreen から呼ばれる。</summary>
        public void Refresh(OwnedCharacterData selectedOperator, IReadOnlyList<OwnedCharacterData> partners)
        {
            bool isOperator = (_data == selectedOperator);
            bool isPartner  = false;
            foreach (var p in partners)
                if (p == _data) { isPartner = true; break; }

            _operatorHighlight?.SetActive(isOperator);
            _partnerHighlight?.SetActive(isPartner);

            if (_stateText != null)
                _stateText.text = isOperator ? "操作" : isPartner ? "パートナー" : "待機";

            if (_setOperatorButton != null)
                _setOperatorButton.interactable = !isOperator;
            if (_togglePartnerButton != null)
                _togglePartnerButton.interactable = !isOperator;
        }

        // ── Private ─────────────────────────────────────────────────────────

        private void UpdateStaticFields()
        {
            if (_nameText  != null) _nameText.text  = $"ID: {_data.characterId}";
            if (_levelText != null) _levelText.text = $"Lv. {_data.currentLevel}";
            if (_rarityText != null) _rarityText.text = _data.rarity.ToString();
            if (_slotSizeText != null) _slotSizeText.text = $"{_data.SlotSize:F1} 枠";
        }
    }
}
