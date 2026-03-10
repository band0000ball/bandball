using System;
using System.Collections.Generic;
using Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// チュートリアル初期キャラクター選択 UI。
    /// ゲーム開始時（コレクション 0 体）に呼び出し、
    /// 指定した候補リストからランダムに 5〜10 体を提示して 1 体を選ばせる。
    ///
    /// 選択完了 → OwnedCharacterCollection に登録して OnCharacterSelected イベントを発火。
    /// UniqueIDManager が利用可能な場合は uniqueId を発行する。
    ///
    /// セットアップ:
    ///   - _cardRoot に _candidateCardPrefab を並べる（HorizontalLayoutGroup 推奨）
    ///   - _candidates に初期候補の OwnedCharacterData を直接設定するか、
    ///     スクリプトから SetCandidates() を呼んで渡す
    ///
    /// depends on: #34a OwnedCharacterData / OwnedCharacterCollection
    /// </summary>
    public class TutorialCharacterSelectUI : MonoBehaviour
    {
        /// <summary>キャラクター選択完了時に発火。選択されたキャラのデータを渡す。</summary>
        public static event System.Action<OwnedCharacterData> OnCharacterSelected;

        [Header("候補リスト（Inspector で直設定 or SetCandidates() で渡す）")]
        [SerializeField] private List<TutorialCandidateData> _candidates = new();

        [Header("UI")]
        [SerializeField] private Transform _cardRoot;
        [SerializeField] private GameObject _candidateCardPrefab;
        [SerializeField] private TextMeshProUGUI _selectionPromptText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private GameObject _selectedHighlightPrefab; // 選択中マーカー（任意）

        [Header("提示体数")]
        [SerializeField, Range(5, 10)] private int _presentCount = 5;

        private readonly List<TutorialCandidateCard> _cards = new();
        private TutorialCandidateData _selectedCandidate;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            BuildCards();
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(Confirm);
            UpdateConfirmButton();
        }

        private void OnDisable()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(Confirm);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>スクリプトから候補を設定してから OnEnable / 手動で BuildCards() を呼ぶ。</summary>
        public void SetCandidates(IEnumerable<TutorialCandidateData> candidates)
        {
            _candidates = new List<TutorialCandidateData>(candidates);
        }

        /// <summary>カードがクリックされたときに TutorialCandidateCard から呼ばれる。</summary>
        public void SelectCandidate(TutorialCandidateData candidate)
        {
            _selectedCandidate = candidate;
            foreach (var card in _cards)
                card.SetSelected(card.Data == candidate);
            UpdateConfirmButton();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void BuildCards()
        {
            foreach (Transform child in _cardRoot)
                Destroy(child.gameObject);
            _cards.Clear();
            _selectedCandidate = null;

            // ランダムに _presentCount 体を抽選
            var pool = new List<TutorialCandidateData>(_candidates);
            var presented = new List<TutorialCandidateData>();
            int count = Mathf.Min(_presentCount, pool.Count);
            for (int i = 0; i < count; i++)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                presented.Add(pool[idx]);
                pool.RemoveAt(idx);
            }

            if (_candidateCardPrefab == null) return;

            foreach (var candidate in presented)
            {
                var go   = Instantiate(_candidateCardPrefab, _cardRoot);
                var card = go.GetComponent<TutorialCandidateCard>();
                if (card != null)
                {
                    card.Initialize(candidate, this);
                    _cards.Add(card);
                }
            }
        }

        private void Confirm()
        {
            if (_selectedCandidate == null) return;

            var col = OwnedCharacterCollection.Instance;
            if (col == null) return;

            // uniqueId 発行
            string uid = System.Guid.NewGuid().ToString();

            var data = new OwnedCharacterData(
                characterId: _selectedCandidate.characterId,
                uniqueId:    uid,
                level:       _selectedCandidate.startLevel,
                rarity:      _selectedCandidate.rarity,
                size:        _selectedCandidate.size,
                behavior:    _selectedCandidate.behavior
            );
            data.baseStatus    = _selectedCandidate.baseStatus;
            data.currentStatus = _selectedCandidate.baseStatus;

            col.TryAdd(data);
            col.SetOperator(data);

            OnCharacterSelected?.Invoke(data);
            gameObject.SetActive(false);
        }

        private void UpdateConfirmButton()
        {
            if (_confirmButton != null)
                _confirmButton.interactable = (_selectedCandidate != null);

            if (_selectionPromptText != null)
                _selectionPromptText.text = _selectedCandidate == null
                    ? "キャラクターを選んでください"
                    : $"ID: {_selectedCandidate.characterId} を選択中";
        }
    }

    // ── 候補データ構造 ────────────────────────────────────────────────────────

    /// <summary>
    /// チュートリアル候補キャラクターのデータ定義。
    /// Inspector で直接設定するか、ScriptableObject 経由で渡す。
    /// </summary>
    [System.Serializable]
    public class TutorialCandidateData
    {
        public int              characterId;
        public string           characterName;
        public int              startLevel   = 1;
        public CharacterRarity  rarity       = CharacterRarity.Common;
        public CharacterSize    size         = CharacterSize.Normal;
        public CharacterBehavior behavior    = CharacterBehavior.Aggressive;
        public CharacterStatus  baseStatus;
        [TextArea(2, 4)]
        public string           description;
        public Sprite           portrait;
    }
}
