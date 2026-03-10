using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level
{
    /// <summary>
    /// Resources/AbilityTrees/ 以下の CSV 3ファイルを起動時に一度だけ読み込み、
    /// 全ノード定義を Dictionary で保持する静的ローダー。
    ///
    /// CSV フォーマット（ヘッダー行あり、列順固定）:
    ///   nodeId, name, apCost, effectType, effectValue, prerequisites, categoryId, characterId, tier
    ///
    /// - prerequisites: パイプ区切り（例: node_hp1|node_atk1）、なければ空
    /// - categoryId: カテゴリノードのみ設定、それ以外は空
    /// - characterId: キャラ固有ノードのみ設定、それ以外は 0
    /// - effectType == UnlockSkill の場合、effectValue 列にスキルGUIDを格納する
    /// </summary>
    public static class AbilityTreeLoader
    {
        private const string CommonCsvPath   = "AbilityTrees/common_nodes";
        private const string CategoryCsvPath = "AbilityTrees/category_nodes";
        private const string CharacterCsvPath = "AbilityTrees/character_nodes";

        private static Dictionary<string, AbilityNodeDef> _allNodes;

        public static IReadOnlyDictionary<string, AbilityNodeDef> AllNodes
        {
            get
            {
                if (_allNodes == null) LoadAll();
                return _allNodes;
            }
        }

        /// <summary>
        /// 指定キャラクターが参照できる全ノード（共通 + カテゴリ + 個人）を返す。
        /// </summary>
        public static List<AbilityNodeDef> GetNodesForCharacter(int characterId, string categoryId)
        {
            if (_allNodes == null) LoadAll();

            return _allNodes.Values
                .Where(n =>
                    (n.CharacterId == 0 && string.IsNullOrEmpty(n.CategoryId)) || // 共通
                    n.CategoryId == categoryId                                   || // カテゴリ
                    n.CharacterId == characterId)                                   // 個人
                .ToList();
        }

        // ── Private ─────────────────────────────────────────────────────────

        private static void LoadAll()
        {
            _allNodes = new Dictionary<string, AbilityNodeDef>();
            LoadCsv(CommonCsvPath);
            LoadCsv(CategoryCsvPath);
            LoadCsv(CharacterCsvPath);
        }

        private static void LoadCsv(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogWarning($"[AbilityTreeLoader] CSV not found: Resources/{resourcePath}.csv");
                return;
            }

            var lines = asset.text.Split('\n');
            for (int i = 1; i < lines.Length; i++) // ヘッダーをスキップ
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var node = ParseLine(line);
                if (node != null)
                    _allNodes[node.NodeId] = node;
            }
        }

        private static AbilityNodeDef ParseLine(string line)
        {
            // スプレッドシートの空末尾カラムに対応するため Split limit なし
            var cols = line.Split(',');
            if (cols.Length < 9) return null;

            string nodeId = cols[0].Trim();
            if (string.IsNullOrEmpty(nodeId)) return null;

            try
            {
                var effectTypeStr = cols[3].Trim();
                var effectType = Enum.Parse<AbilityEffectType>(effectTypeStr);
                var effectValueStr = cols[4].Trim();

                var node = new AbilityNodeDef
                {
                    NodeId      = nodeId,
                    Name        = cols[1].Trim(),
                    ApCost      = int.Parse(cols[2].Trim()),
                    EffectType  = effectType,
                    Tier        = int.Parse(cols[8].Trim()),
                };

                // UnlockSkill は effectValue 列をスキルGUIDとして扱う
                if (effectType == AbilityEffectType.UnlockSkill)
                {
                    node.EffectSkillGuid = effectValueStr;
                    node.EffectValue     = 0f;
                }
                else
                {
                    node.EffectValue = float.Parse(effectValueStr);
                }

                // prerequisites（パイプ区切り）
                var prereqStr = cols[5].Trim();
                node.Prerequisites = string.IsNullOrEmpty(prereqStr)
                    ? Array.Empty<string>()
                    : prereqStr.Split('|');

                // categoryId / characterId
                node.CategoryId  = cols[6].Trim();
                node.CharacterId = int.TryParse(cols[7].Trim(), out int cid) ? cid : 0;

                return node;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AbilityTreeLoader] Parse failed (line: \"{line}\"): {e.Message}");
                return null;
            }
        }
    }
}
