using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// チュートリアル初期キャラ選択画面の各候補カード。
    /// TutorialCharacterSelectUI が Instantiate して Initialize() を呼ぶ。
    /// </summary>
    public class TutorialCandidateCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private GameObject _selectedOverlay;
        [SerializeField] private Button _selectButton;

        public TutorialCandidateData Data { get; private set; }

        private TutorialCharacterSelectUI _owner;

        // ── 初期化 ──────────────────────────────────────────────────────────

        public void Initialize(TutorialCandidateData data, TutorialCharacterSelectUI owner)
        {
            Data   = data;
            _owner = owner;

            if (_nameText   != null) _nameText.text   = data.characterName;
            if (_rarityText != null) _rarityText.text  = data.rarity.ToString();
            if (_descText   != null) _descText.text    = data.description;
            if (_portraitImage != null && data.portrait != null)
                _portraitImage.sprite = data.portrait;

            _selectedOverlay?.SetActive(false);

            if (_selectButton != null)
                _selectButton.onClick.AddListener(OnClicked);
        }

        /// <summary>TutorialCharacterSelectUI から選択状態を反映するために呼ばれる。</summary>
        public void SetSelected(bool selected)
        {
            _selectedOverlay?.SetActive(selected);
        }

        // ── Private ─────────────────────────────────────────────────────────

        private void OnClicked()
        {
            _owner?.SelectCandidate(Data);
        }
    }
}
