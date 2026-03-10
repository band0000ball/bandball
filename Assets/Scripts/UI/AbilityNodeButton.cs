using Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// アビリティツリーの各ノードボタン。
    /// AbilityTreePanel が Instantiate して Initialize() を呼ぶ。
    ///
    /// 状態:
    ///   Unlocked  : 習得済み（緑ハイライト）
    ///   Available : 解放可能（AP足りる＋前提クリア）
    ///   Blocked   : 前提未クリアまたはAP不足（グレーアウト）
    /// </summary>
    public class AbilityNodeButton : MonoBehaviour
    {
        [Header("テキスト")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _effectText;
        [SerializeField] private TextMeshProUGUI _tierText;

        [Header("状態表示")]
        [SerializeField] private GameObject _unlockedOverlay;   // 習得済みマーク
        [SerializeField] private GameObject _blockedOverlay;    // 解放不可オーバーレイ

        [Header("ボタン")]
        [SerializeField] private Button _button;

        public string NodeId { get; private set; }

        private AbilityNodeDef _node;
        private AbilityTreePanel _panel;

        // ── 初期化 ──────────────────────────────────────────────────────────

        public void Initialize(AbilityNodeDef node, AbilityTreePanel panel)
        {
            _node  = node;
            _panel = panel;
            NodeId = node.NodeId;

            if (_nameText  != null) _nameText.text  = node.Name;
            if (_costText  != null) _costText.text  = $"{node.ApCost} AP";
            if (_tierText  != null) _tierText.text  = $"Tier {node.Tier}";
            if (_effectText != null) _effectText.text = FormatEffect(node);

            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        /// <summary>選択キャラクターの状態に合わせて表示を更新する。</summary>
        public void Refresh(bool isUnlocked, bool canUnlock)
        {
            _unlockedOverlay?.SetActive(isUnlocked);
            _blockedOverlay?.SetActive(!isUnlocked && !canUnlock);

            if (_button != null)
                _button.interactable = !isUnlocked && canUnlock;
        }

        // ── Private ─────────────────────────────────────────────────────────

        private void OnClicked()
        {
            _panel?.OnNodeClicked(_node.NodeId);
        }

        private static string FormatEffect(AbilityNodeDef node)
        {
            return node.EffectType switch
            {
                AbilityEffectType.MaxHealth           => $"最大HP +{node.EffectValue:F0}",
                AbilityEffectType.MaxStamina          => $"最大スタミナ +{node.EffectValue:F0}",
                AbilityEffectType.AttackPower         => $"攻撃力 +{node.EffectValue:F1}",
                AbilityEffectType.DefensePower        => $"防御力 +{node.EffectValue:F1}",
                AbilityEffectType.MoveSpeed           => $"移動速度 +{node.EffectValue:F2}",
                AbilityEffectType.BaseAttributePower  => $"属性攻撃力 +{node.EffectValue:F1}",
                AbilityEffectType.BaseResistancePower => $"属性耐性 +{node.EffectValue:F1}",
                AbilityEffectType.UnlockSkill         => "スキル解放",
                _                                     => node.EffectType.ToString(),
            };
        }
    }
}
