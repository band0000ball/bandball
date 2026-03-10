using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// パーティステータスUI の各メンバースロット。
    /// PartyStatusUI が毎フレーム Show() / Hide() を呼んで状態を更新する。
    /// Inspector で各 UI 要素を参照設定すること。
    /// </summary>
    public class PartyMemberSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private GameObject _operatorMark;   // 操作キャラ表示マーク
        [SerializeField] private GameObject _deadOverlay;    // 死亡時に表示するオーバーレイ
        [SerializeField] private Button _switchButton;       // クリックで操作切り替え

        private System.Action _onClicked;

        private void Awake()
        {
            if (_switchButton != null)
                _switchButton.onClick.AddListener(OnButtonClicked);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>スロットを表示して情報を更新する。</summary>
        /// <param name="characterLabel">キャラクター識別ラベル（名前 or ID）</param>
        /// <param name="hp">現在HP</param>
        /// <param name="maxHp">最大HP</param>
        /// <param name="isDead">死亡しているか</param>
        /// <param name="isOperator">現在の操作キャラか</param>
        /// <param name="onClicked">スロットクリック時のコールバック（null の場合はボタン無効）</param>
        public void Show(string characterLabel, float hp, float maxHp, bool isDead, bool isOperator, System.Action onClicked)
        {
            gameObject.SetActive(true);

            if (_nameText != null)  _nameText.text = characterLabel;

            if (_hpSlider != null)
            {
                _hpSlider.maxValue = Mathf.Max(1f, maxHp);
                _hpSlider.value    = Mathf.Max(0f, hp);
            }

            if (_hpText != null)
                _hpText.text = $"{Mathf.Max(0, (int)hp)} / {(int)maxHp}";

            _operatorMark?.SetActive(isOperator);
            _deadOverlay?.SetActive(isDead);

            _onClicked = onClicked;
            if (_switchButton != null)
                _switchButton.interactable = onClicked != null;
        }

        /// <summary>スロットを非表示にする。</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _onClicked = null;
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnButtonClicked()
        {
            _onClicked?.Invoke();
        }
    }
}
