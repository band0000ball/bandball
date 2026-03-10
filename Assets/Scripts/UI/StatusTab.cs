using Character;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// ステータスタブ。所持キャラクターのステータスを表示する。
    /// Refresh() 呼び出し時に OwnedCharacterCollection から現在のデータを取得する。
    /// SetTarget() でキャラを選択すると詳細が更新される（キャラリスト UI と連携）。
    ///
    /// depends on: #34a OwnedCharacterData / OwnedCharacterCollection
    /// </summary>
    public class StatusTab : MonoBehaviour
    {
        [Header("キャラ情報")]
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _sizeText;
        [SerializeField] private TextMeshProUGUI _behaviorText;

        [Header("ステータス数値")]
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _staminaText;
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _defenseText;
        [SerializeField] private TextMeshProUGUI _moveSpeedText;
        [SerializeField] private TextMeshProUGUI _attributePowerText;
        [SerializeField] private TextMeshProUGUI _resistancePowerText;

        [Header("出撃情報")]
        [SerializeField] private TextMeshProUGUI _isActiveText;
        [SerializeField] private TextMeshProUGUI _alchemizableText;

        private OwnedCharacterData _displayTarget;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>タブ表示時に MenuCanvas から呼ばれる。未選択なら最初のアクティブキャラを表示。</summary>
        public void Refresh()
        {
            var col = OwnedCharacterCollection.Instance;
            if (col == null) return;

            if (_displayTarget == null)
                SelectDefault(col);

            UpdateDisplay();
        }

        /// <summary>キャラ選択リストから呼ぶ。指定キャラの詳細に切り替える。</summary>
        public void SetTarget(OwnedCharacterData data)
        {
            _displayTarget = data;
            UpdateDisplay();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void SelectDefault(OwnedCharacterCollection col)
        {
            // 操作キャラ優先 → 最初のアクティブキャラ → コレクション先頭
            if (col.OperatingCharacter != null)
            {
                _displayTarget = col.OperatingCharacter;
                return;
            }
            foreach (var c in col.Characters)
            {
                if (c.isActive) { _displayTarget = c; return; }
            }
            if (col.Characters.Count > 0)
                _displayTarget = col.Characters[0];
        }

        private void UpdateDisplay()
        {
            if (_displayTarget == null)
            {
                ClearDisplay();
                return;
            }

            var s = _displayTarget.currentStatus;

            SetText(_characterNameText, $"ID: {_displayTarget.characterId}");
            SetText(_levelText,         $"Lv. {_displayTarget.currentLevel}");
            SetText(_rarityText,        _displayTarget.rarity.ToString());
            SetText(_sizeText,          _displayTarget.size.ToString());
            SetText(_behaviorText,      _displayTarget.behavior.ToString());

            SetText(_hpText,             s.maxHp.ToString());
            SetText(_staminaText,        s.maxStamina.ToString());
            SetText(_attackText,         s.attackPower.ToString("F1"));
            SetText(_defenseText,        s.defensePower.ToString("F1"));
            SetText(_moveSpeedText,      s.moveSpeed.ToString("F2"));
            SetText(_attributePowerText, s.baseAttributePower.ToString("F1"));
            SetText(_resistancePowerText,s.baseResistancePower.ToString("F1"));

            SetText(_isActiveText,       _displayTarget.isActive ? "出撃中" : "待機");
            SetText(_alchemizableText,   _displayTarget.isAlchemizable ? "可" : "不可");
        }

        private void ClearDisplay()
        {
            string dash = "---";
            SetText(_characterNameText, dash);
            SetText(_levelText,         dash);
            SetText(_rarityText,        dash);
            SetText(_sizeText,          dash);
            SetText(_behaviorText,      dash);
            SetText(_hpText,            dash);
            SetText(_staminaText,       dash);
            SetText(_attackText,        dash);
            SetText(_defenseText,       dash);
            SetText(_moveSpeedText,     dash);
            SetText(_attributePowerText,dash);
            SetText(_resistancePowerText,dash);
            SetText(_isActiveText,      dash);
            SetText(_alchemizableText,  dash);
        }

        private static void SetText(TextMeshProUGUI label, string value)
        {
            if (label != null) label.text = value;
        }
    }
}
