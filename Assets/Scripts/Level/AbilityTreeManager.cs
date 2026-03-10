using System.Collections.Generic;
using System.Linq;
using Buff;
using Character;
using Commons;
using Manager;

namespace Level
{
    /// <summary>
    /// キャラクター1体分のアビリティツリー習得状態を管理するクラス。
    /// CharacterControl が所有し、AP消費・効果適用・リセットを担う。
    ///
    /// 設計:
    ///   - ノード定義 (AbilityNodeDef) は AbilityTreeLoader から取得（静的・共有）
    ///   - 習得状態 (_unlockedNodes) はこのインスタンスが保持（動的・個別）
    ///   - ステータス効果は CharacterControl.AddBuff() / DecValueBuffByType() で反映
    ///   - スキル解放は _unlockedSkills で管理（IsSkillUnlocked() で照会）
    /// </summary>
    public class AbilityTreeManager
    {
        private readonly CharacterControl _control;
        private readonly List<AbilityNodeDef> _availableNodes;

        /// <summary>習得済みノード: nodeId → 消費AP（リセット時のAP返還量計算に使用）</summary>
        private readonly Dictionary<string, int> _unlockedNodes = new();

        /// <summary>解放済みスキルGUIDセット</summary>
        private readonly HashSet<string> _unlockedSkills = new();

        /// <summary>現在の累計消費AP</summary>
        public int SpentAP => _unlockedNodes.Values.Sum();

        /// <summary>習得済みノード（読み取り専用、UI表示用）</summary>
        public IReadOnlyDictionary<string, int> UnlockedNodes => _unlockedNodes;

        /// <summary>このキャラクターが参照できる全ノード定義（UI表示用）</summary>
        public IReadOnlyList<AbilityNodeDef> AvailableNodes => _availableNodes;

        public AbilityTreeManager(CharacterControl control, int characterId, string categoryId)
        {
            _control        = control;
            _availableNodes = AbilityTreeLoader.GetNodesForCharacter(characterId, categoryId);
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// ノードを習得できるか判定する。
        /// </summary>
        public bool CanUnlock(string nodeId)
        {
            if (_unlockedNodes.ContainsKey(nodeId)) return false;

            var node = GetNodeDef(nodeId);
            if (node == null) return false;

            if (_control.GetAbilityPoints() < node.ApCost) return false;

            foreach (var prereq in node.Prerequisites)
                if (!_unlockedNodes.ContainsKey(prereq)) return false;

            return true;
        }

        /// <summary>
        /// ノードの習得を試みる。成功時は AP を消費し効果を適用する。
        /// </summary>
        public bool TryUnlock(string nodeId)
        {
            if (!CanUnlock(nodeId)) return false;

            var node = GetNodeDef(nodeId);
            if (!_control.TrySpendAbilityPoints(node.ApCost)) return false;

            _unlockedNodes[nodeId] = node.ApCost;
            ApplyEffect(node);
            return true;
        }

        /// <summary>
        /// 全ノードをリセットし、消費APを返還する。
        /// リセットコスト = 消費AP × GameBalance.ABILITY_RESET_GOLD_RATE ゴールド。
        /// </summary>
        public bool TryReset()
        {
            if (_unlockedNodes.Count == 0) return true;

            int resetCost = SpentAP * GameBalance.ABILITY_RESET_GOLD_RATE;
            if (PlayerWallet.Instance == null ||
                !PlayerWallet.Instance.TrySpend(resetCost))
                return false;

            foreach (var nodeId in _unlockedNodes.Keys.ToList())
            {
                var node = GetNodeDef(nodeId);
                if (node != null) RemoveEffect(node);
            }

            int returnedAP = SpentAP;
            _unlockedNodes.Clear();
            _unlockedSkills.Clear();
            _control.AddAbilityPoints(returnedAP);
            return true;
        }

        /// <summary>
        /// ノードが習得済みかを返す。
        /// </summary>
        public bool IsUnlocked(string nodeId) => _unlockedNodes.ContainsKey(nodeId);

        /// <summary>
        /// スキルが解放済みかを返す（スキル使用時の判定に使用）。
        /// </summary>
        public bool IsSkillUnlocked(string skillGuid) => _unlockedSkills.Contains(skillGuid);

        /// <summary>
        /// リセットに必要なゴールドを返す（UI表示用）。
        /// </summary>
        public int GetResetCost() => SpentAP * GameBalance.ABILITY_RESET_GOLD_RATE;

        // ── Private ─────────────────────────────────────────────────────────

        private AbilityNodeDef GetNodeDef(string nodeId)
            => _availableNodes.FirstOrDefault(n => n.NodeId == nodeId);

        private void ApplyEffect(AbilityNodeDef node)
        {
            if (node.EffectType == AbilityEffectType.UnlockSkill)
            {
                if (!string.IsNullOrEmpty(node.EffectSkillGuid))
                    _unlockedSkills.Add(node.EffectSkillGuid);
                return;
            }

            // ステータス系: 永続Buffとして加算
            // float.MaxValue は BuffManager の Update で精度の問題から実質減算されない
            _control.AddBuff(EffectTypeToBuffType(node.EffectType), node.EffectValue, float.MaxValue);
        }

        private void RemoveEffect(AbilityNodeDef node)
        {
            if (node.EffectType == AbilityEffectType.UnlockSkill)
            {
                _unlockedSkills.Remove(node.EffectSkillGuid);
                return;
            }

            _control.DecValueBuffByType(EffectTypeToBuffType(node.EffectType), node.EffectValue);
        }

        private static BuffType EffectTypeToBuffType(AbilityEffectType effectType) => effectType switch
        {
            AbilityEffectType.MaxHealth            => BuffType.Health,
            AbilityEffectType.MaxStamina           => BuffType.Stamina,
            AbilityEffectType.AttackPower          => BuffType.AttackPower,
            AbilityEffectType.DefensePower         => BuffType.DefensePower,
            AbilityEffectType.MoveSpeed            => BuffType.MoveSpeed,
            AbilityEffectType.BaseAttributePower   => BuffType.BaseAttributePower,
            AbilityEffectType.BaseResistancePower  => BuffType.BaseResistancePower,
            _ => throw new System.ArgumentOutOfRangeException(nameof(effectType), effectType, null),
        };
    }
}
