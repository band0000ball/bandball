using System.Collections.Generic;
using Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 出撃画面UI。ステージ開始前に操作キャラ1体とパートナーを選択する。
    ///
    /// 選択完了 → OwnedCharacterCollection.SetupDeployment() を呼び出し、
    /// OnDeploymentConfirmed イベントを発火してステージ遷移を通知する。
    ///
    /// 装備割り当ては TODO（アイテムシステム実装後に対応）。
    ///
    /// セットアップ:
    ///   - _characterListRoot 配下に _characterCardPrefab を並べる（ScrollRect 推奨）
    ///   - _deployButton の OnClick に Confirm() を設定
    ///
    /// depends on: #34a OwnedCharacterCollection, #34b PlayerCharacterController
    /// </summary>
    public class DeployScreen : MonoBehaviour
    {
        public static DeployScreen Instance { get; private set; }

        /// <summary>出撃確定時に発火。StageManager や GameSceneManager が購読してシーン遷移を開始する。</summary>
        public static event System.Action OnDeploymentConfirmed;

        [Header("キャラクターリスト")]
        [SerializeField] private Transform _characterListRoot;
        [SerializeField] private GameObject _characterCardPrefab;

        [Header("スロット情報 UI")]
        [SerializeField] private TextMeshProUGUI _operatorSlotText;
        [SerializeField] private TextMeshProUGUI _partnerSlotsText;
        [SerializeField] private TextMeshProUGUI _errorText;

        [Header("ボタン")]
        [SerializeField] private Button _deployButton;

        // ── 選択状態 ─────────────────────────────────────────────────────────

        private OwnedCharacterData _selectedOperator;
        private readonly List<OwnedCharacterData> _selectedPartners = new();

        private readonly List<DeployCharacterCard> _cards = new();

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            BuildCharacterList();
            RefreshSlotDisplay();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>キャラクターカードから「操作キャラに設定」ボタンで呼ばれる。</summary>
        public void SetOperator(OwnedCharacterData data)
        {
            if (data == null) return;

            // パートナーから外す（操作キャラはパートナー兼務不可）
            _selectedPartners.Remove(data);

            _selectedOperator = data;

            RefreshCards();
            RefreshSlotDisplay();
        }

        /// <summary>キャラクターカードから「パートナー追加/解除」ボタンで呼ばれる。</summary>
        public void TogglePartner(OwnedCharacterData data)
        {
            if (data == null || data == _selectedOperator) return;

            if (_selectedPartners.Contains(data))
            {
                _selectedPartners.Remove(data);
            }
            else
            {
                float usedSlots = CalcUsedPartnerSlots();
                if (usedSlots + data.SlotSize > 2.0f)
                {
                    ShowError("パートナースロットが満員です（最大 2.0 枠）");
                    return;
                }
                _selectedPartners.Add(data);
            }

            ClearError();
            RefreshCards();
            RefreshSlotDisplay();
        }

        /// <summary>出撃確定ボタンから呼ばれる。</summary>
        public void Confirm()
        {
            if (_selectedOperator == null)
            {
                ShowError("操作キャラクターを選択してください");
                return;
            }

            var col = OwnedCharacterCollection.Instance;
            if (col == null) return;

            col.SetupDeployment(_selectedOperator, _selectedPartners);
            ClearError();

            OnDeploymentConfirmed?.Invoke();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void BuildCharacterList()
        {
            // 既存カードを全削除
            foreach (Transform child in _characterListRoot)
                Destroy(child.gameObject);
            _cards.Clear();

            _selectedOperator = null;
            _selectedPartners.Clear();

            var col = OwnedCharacterCollection.Instance;
            if (col == null || _characterCardPrefab == null) return;

            foreach (var charData in col.Characters)
            {
                var go = Instantiate(_characterCardPrefab, _characterListRoot);
                var card = go.GetComponent<DeployCharacterCard>();
                if (card != null)
                {
                    card.Initialize(charData, this);
                    _cards.Add(card);
                }
            }
        }

        private void RefreshCards()
        {
            foreach (var card in _cards)
                card.Refresh(_selectedOperator, _selectedPartners);
        }

        private void RefreshSlotDisplay()
        {
            if (_operatorSlotText != null)
                _operatorSlotText.text = _selectedOperator != null
                    ? $"操作: ID {_selectedOperator.characterId} / Lv.{_selectedOperator.currentLevel}"
                    : "操作: 未選択";

            float used = CalcUsedPartnerSlots();
            if (_partnerSlotsText != null)
                _partnerSlotsText.text = $"パートナー: {used:F1} / 2.0 枠";

            if (_deployButton != null)
                _deployButton.interactable = (_selectedOperator != null);
        }

        private float CalcUsedPartnerSlots()
        {
            float total = 0f;
            foreach (var p in _selectedPartners)
                total += p.SlotSize;
            return total;
        }

        private void ShowError(string msg)
        {
            if (_errorText != null) { _errorText.text = msg; _errorText.gameObject.SetActive(true); }
        }

        private void ClearError()
        {
            if (_errorText != null) _errorText.gameObject.SetActive(false);
        }
    }
}
