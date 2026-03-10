using System;
using System.Collections.Generic;
using Commons;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// 属性相性データを管理するScriptableObject
    /// Unityエディタ上でバランス調整が可能
    /// </summary>
    [CreateAssetMenu(fileName = "AttributeAffinity", menuName = "PictureStory/Attribute Affinity Data")]
    public class AttributeAffinityData : ScriptableObject
    {
        #region Nested Types

        [Serializable]
        public class AffinityEntry
        {
            [Tooltip("攻撃側の属性")]
            public AttributeMagnification.Attribute attacker;

            [Tooltip("防御側の属性")]
            public AttributeMagnification.Attribute defender;

            [Tooltip("ダメージ倍率（1.0 = 等倍, 2.0 = 弱点, 0.5 = 耐性）")]
            [Range(0f, 3f)]
            public float multiplier = 1f;

            [Tooltip("設定理由（メモ用）")]
            [TextArea(1, 2)]
            public string reason;
        }

        #endregion

        #region Serialized Fields

        [Header("属性相性設定")]
        [Tooltip("属性ごとの相性リスト")]
        [SerializeField]
        private List<AffinityEntry> _affinities = new();

        [Header("デフォルト設定")]
        [Tooltip("相性が定義されていない場合のデフォルト倍率")]
        [SerializeField]
        private float _defaultMultiplier = 1f;

        #endregion

        #region Private Fields

        // キャッシュ用
        private Dictionary<(AttributeMagnification.Attribute, AttributeMagnification.Attribute), float> _cache;
        private bool _cacheBuilt;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            BuildCache();
        }

        private void OnValidate()
        {
            // エディタで変更があった場合にキャッシュを再構築
            _cacheBuilt = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 属性相性倍率を取得する
        /// </summary>
        /// <param name="attacker">攻撃側の属性</param>
        /// <param name="defender">防御側の属性</param>
        /// <returns>ダメージ倍率</returns>
        public float GetMultiplier(AttributeMagnification.Attribute attacker, AttributeMagnification.Attribute defender)
        {
            if (!_cacheBuilt)
            {
                BuildCache();
            }

            // Noneの場合は常に等倍
            if (attacker == AttributeMagnification.Attribute.None ||
                defender == AttributeMagnification.Attribute.None)
            {
                return 1f;
            }

            if (_cache.TryGetValue((attacker, defender), out float multiplier))
            {
                return multiplier;
            }

            return _defaultMultiplier;
        }

        /// <summary>
        /// 既存のAttributeMagnificationとの互換性を保つための計算メソッド
        /// </summary>
        public float Calculate(float[] defenderAttributes, AttributeMagnification.Attribute attackerAttribute)
        {
            if (attackerAttribute == AttributeMagnification.Attribute.None)
            {
                return 1f;
            }

            float total = 0f;

            for (int i = 0; i < defenderAttributes.Length && i < 10; i++)
            {
                var defenderAttr = (AttributeMagnification.Attribute)i;
                float multiplier = GetMultiplier(attackerAttribute, defenderAttr);
                total += multiplier * defenderAttributes[i];
            }

            return total;
        }

        /// <summary>
        /// 弱点属性かどうかを判定
        /// </summary>
        public bool IsWeakness(AttributeMagnification.Attribute attacker, AttributeMagnification.Attribute defender)
        {
            return GetMultiplier(attacker, defender) > 1f;
        }

        /// <summary>
        /// 耐性属性かどうかを判定
        /// </summary>
        public bool IsResistance(AttributeMagnification.Attribute attacker, AttributeMagnification.Attribute defender)
        {
            return GetMultiplier(attacker, defender) < 1f;
        }

        #endregion

        #region Private Methods

        private void BuildCache()
        {
            _cache = new Dictionary<(AttributeMagnification.Attribute, AttributeMagnification.Attribute), float>();

            foreach (var entry in _affinities)
            {
                var key = (entry.attacker, entry.defender);
                if (!_cache.ContainsKey(key))
                {
                    _cache[key] = entry.multiplier;
                }
                else
                {
                    Debug.LogWarning($"AttributeAffinityData: 重複エントリ検出 - {entry.attacker} vs {entry.defender}");
                }
            }

            _cacheBuilt = true;
        }

        #endregion

        #region Editor Utility

#if UNITY_EDITOR
        /// <summary>
        /// デフォルトの相性テーブルを初期化する（エディタ用）
        /// </summary>
        [ContextMenu("Initialize Default Affinities")]
        private void InitializeDefaultAffinities()
        {
            _affinities.Clear();

            // 既存のMagnificationテーブルに基づいて初期化
            // Frame (火) の相性
            AddAffinity(AttributeMagnification.Attribute.Frame, AttributeMagnification.Attribute.Aqua, 2.0f, "火は水に弱い");
            AddAffinity(AttributeMagnification.Attribute.Frame, AttributeMagnification.Attribute.Ground, 0.5f, "火は土に強い");
            AddAffinity(AttributeMagnification.Attribute.Frame, AttributeMagnification.Attribute.Ice, 0.5f, "火は氷に強い");

            // Aqua (水) の相性
            AddAffinity(AttributeMagnification.Attribute.Aqua, AttributeMagnification.Attribute.Frame, 0.5f, "水は火に強い");
            AddAffinity(AttributeMagnification.Attribute.Aqua, AttributeMagnification.Attribute.Electric, 2.0f, "水は電気に弱い");
            AddAffinity(AttributeMagnification.Attribute.Aqua, AttributeMagnification.Attribute.Oil, 0.5f, "水は油に強い");

            // Electric (電気) の相性
            AddAffinity(AttributeMagnification.Attribute.Electric, AttributeMagnification.Attribute.Aqua, 0.5f, "電気は水に強い");
            AddAffinity(AttributeMagnification.Attribute.Electric, AttributeMagnification.Attribute.Plant, 2.0f, "電気は植物に弱い");
            AddAffinity(AttributeMagnification.Attribute.Electric, AttributeMagnification.Attribute.Wind, 0.5f, "電気は風に強い");

            // Plant (植物) の相性
            AddAffinity(AttributeMagnification.Attribute.Plant, AttributeMagnification.Attribute.Electric, 0.5f, "植物は電気に強い");
            AddAffinity(AttributeMagnification.Attribute.Plant, AttributeMagnification.Attribute.Ground, 2.0f, "植物は土に弱い");
            AddAffinity(AttributeMagnification.Attribute.Plant, AttributeMagnification.Attribute.Toxin, 0.5f, "植物は毒に強い");

            // Ground (土) の相性
            AddAffinity(AttributeMagnification.Attribute.Ground, AttributeMagnification.Attribute.Frame, 2.0f, "土は火に弱い");
            AddAffinity(AttributeMagnification.Attribute.Ground, AttributeMagnification.Attribute.Plant, 0.5f, "土は植物に強い");
            AddAffinity(AttributeMagnification.Attribute.Ground, AttributeMagnification.Attribute.Spirit, 0.5f, "土は精神に強い");

            // Ice (氷) の相性
            AddAffinity(AttributeMagnification.Attribute.Ice, AttributeMagnification.Attribute.Frame, 0.5f, "氷は火に強い");
            AddAffinity(AttributeMagnification.Attribute.Ice, AttributeMagnification.Attribute.Oil, 0.5f, "氷は油に強い");
            AddAffinity(AttributeMagnification.Attribute.Ice, AttributeMagnification.Attribute.Spirit, 2.0f, "氷は精神に弱い");

            // Oil (油) の相性
            AddAffinity(AttributeMagnification.Attribute.Oil, AttributeMagnification.Attribute.Aqua, 0.5f, "油は水に強い");
            AddAffinity(AttributeMagnification.Attribute.Oil, AttributeMagnification.Attribute.Ice, 2.0f, "油は氷に弱い");
            AddAffinity(AttributeMagnification.Attribute.Oil, AttributeMagnification.Attribute.Wind, 0.5f, "油は風に強い");

            // Wind (風) の相性
            AddAffinity(AttributeMagnification.Attribute.Wind, AttributeMagnification.Attribute.Electric, 0.5f, "風は電気に強い");
            AddAffinity(AttributeMagnification.Attribute.Wind, AttributeMagnification.Attribute.Oil, 2.0f, "風は油に弱い");
            AddAffinity(AttributeMagnification.Attribute.Wind, AttributeMagnification.Attribute.Toxin, 0.5f, "風は毒に強い");

            // Toxin (毒) の相性
            AddAffinity(AttributeMagnification.Attribute.Toxin, AttributeMagnification.Attribute.Plant, 0.5f, "毒は植物に強い");
            AddAffinity(AttributeMagnification.Attribute.Toxin, AttributeMagnification.Attribute.Wind, 2.0f, "毒は風に弱い");
            AddAffinity(AttributeMagnification.Attribute.Toxin, AttributeMagnification.Attribute.Spirit, 0.5f, "毒は精神に強い");

            // Spirit (精神) の相性
            AddAffinity(AttributeMagnification.Attribute.Spirit, AttributeMagnification.Attribute.Ground, 0.5f, "精神は土に強い");
            AddAffinity(AttributeMagnification.Attribute.Spirit, AttributeMagnification.Attribute.Ice, 0.5f, "精神は氷に強い");
            AddAffinity(AttributeMagnification.Attribute.Spirit, AttributeMagnification.Attribute.Toxin, 2.0f, "精神は毒に弱い");

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("AttributeAffinityData: デフォルト相性を初期化しました");
        }

        private void AddAffinity(AttributeMagnification.Attribute attacker, AttributeMagnification.Attribute defender, float multiplier, string reason)
        {
            _affinities.Add(new AffinityEntry
            {
                attacker = attacker,
                defender = defender,
                multiplier = multiplier,
                reason = reason
            });
        }
#endif

        #endregion
    }
}
