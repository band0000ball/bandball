using System.Collections.Generic;
using Character;
using Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// アビリティツリーパネル。キャラクター1体分のノードを Tier 別に表示し、
    /// AP消費によるノード習得とリセットを管理する。
    ///
    /// AbilityTab._treeStubPanel の代わりに使用する。
    /// SetCharacter() でキャラクターをセットしてから Refresh() を呼ぶこと。
    ///
    /// Tier ルート GameObject の配列 (_tierRoots) を Inspector で設定する。
    ///   - インデックスが Tier 値と一致する（Tier1 → [1]、Tier2 → [2]、Tier3 → [3]）
    ///   - [0] は Tier0（コアノード）用。不要なら空でも可
    ///
    /// depends on: #13 AbilityTreeManager / AbilityTreeLoader
    /// </summary>
    public class AbilityTreePanel : MonoBehaviour
    {
        [Header("Tier 別ノード配置ルート（インデックス = Tier 値）")]
        [SerializeField] private Transform[] _tierRoots;

        [Header("ノードボタンプレハブ")]
        [SerializeField] private GameObject _nodeButtonPrefab;

        [Header("AP / リセット UI")]
        [SerializeField] private TextMeshProUGUI _apText;
        [SerializeField] private TextMeshProUGUI _resetCostText;
        [SerializeField] private Button _resetButton;
        [SerializeField] private TextMeshProUGUI _messageText;

        private CharacterControl _character;
        private AbilityTreeManager _manager;
        private readonly List<AbilityNodeButton> _nodeButtons = new();

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetClicked);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>表示対象キャラクターを設定してツリーを再構築する。</summary>
        public void SetCharacter(CharacterControl character)
        {
            _character = character;
            _manager   = character?.GetAbilityTreeManager();
            BuildTree();
        }

        /// <summary>現在の習得状態に合わせてボタン表示を更新する（タブ切替時などに呼ぶ）。</summary>
        public void Refresh()
        {
            if (_manager == null)
            {
                ClearMessage();
                return;
            }

            foreach (var btn in _nodeButtons)
                RefreshButton(btn);

            UpdateAPDisplay();
        }

        /// <summary>ノードボタンから呼ばれる（OnClicked の委譲先）。</summary>
        public void OnNodeClicked(string nodeId)
        {
            if (_manager == null) return;

            bool success = _manager.TryUnlock(nodeId);
            ShowMessage(success ? "習得しました！" : "習得できません（AP不足または前提条件未達）");
            Refresh();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void BuildTree()
        {
            // 既存ボタンをすべて破棄
            foreach (var btn in _nodeButtons)
                if (btn != null) Destroy(btn.gameObject);
            _nodeButtons.Clear();

            if (_manager == null || _nodeButtonPrefab == null) { Refresh(); return; }

            foreach (var node in _manager.AvailableNodes)
            {
                var root = GetTierRoot(node.Tier);
                if (root == null) continue;

                var go  = Instantiate(_nodeButtonPrefab, root);
                var btn = go.GetComponent<AbilityNodeButton>();
                if (btn != null)
                {
                    btn.Initialize(node, this);
                    _nodeButtons.Add(btn);
                }
            }

            Refresh();
        }

        private void RefreshButton(AbilityNodeButton btn)
        {
            if (btn == null || _manager == null) return;

            bool isUnlocked = _manager.IsUnlocked(btn.NodeId);
            bool canUnlock  = _manager.CanUnlock(btn.NodeId);
            btn.Refresh(isUnlocked, canUnlock);
        }

        private void UpdateAPDisplay()
        {
            if (_character != null && _apText != null)
                _apText.text = $"AP: {_character.GetAbilityPoints()}";

            if (_manager != null && _resetCostText != null)
                _resetCostText.text = $"リセット: {_manager.GetResetCost()} G";

            if (_resetButton != null)
                _resetButton.interactable = (_manager != null && _manager.SpentAP > 0);
        }

        private void OnResetClicked()
        {
            if (_manager == null) return;
            bool success = _manager.TryReset();
            ShowMessage(success ? "リセットしました。" : "リセットできません（通貨不足）");
            Refresh();
        }

        private Transform GetTierRoot(int tier)
        {
            if (_tierRoots == null || tier < 0 || tier >= _tierRoots.Length)
                return _tierRoots != null && _tierRoots.Length > 0 ? _tierRoots[0] : null;
            return _tierRoots[tier];
        }

        private void ShowMessage(string msg)
        {
            if (_messageText != null) { _messageText.text = msg; _messageText.gameObject.SetActive(true); }
        }

        private void ClearMessage()
        {
            if (_messageText != null) _messageText.gameObject.SetActive(false);
        }
    }
}
