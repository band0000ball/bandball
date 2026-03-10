using System;
using Character;
using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 敵 AI の行動スコアリングシステム。
    /// 状況を評価してスコアが最高の AttackPattern を返す。
    ///
    /// スコア計算の考慮要素:
    ///   - 距離が攻撃レンジ中央に近いほど高スコア
    ///   - HP が低いほどスコアが下がる
    ///   - 個別クールダウン中のパターンは除外
    /// </summary>
    public static class ActionSelector
    {
        /// <summary>
        /// 最適な攻撃パターンを選択して返す。
        /// 条件に合うパターンがなければ null を返す。
        /// </summary>
        public static AttackPattern SelectBest(
            AttackPattern[] patterns,
            float distanceToTarget,
            float hpRatio,
            Func<string, bool> isPatternReady)
        {
            if (patterns == null || patterns.Length == 0) return null;

            AttackPattern best = null;
            float bestScore = float.MinValue;

            foreach (var p in patterns)
            {
                // 距離チェック
                if (distanceToTarget < p.minRange || distanceToTarget > p.maxRange) continue;

                // クールダウンチェック
                if (!isPatternReady(p.patternName)) continue;

                // スコア計算
                float score = p.baseScore;

                // レンジ中央からの距離でペナルティ
                float rangeMid = (p.minRange + p.maxRange) * 0.5f;
                score -= Mathf.Abs(distanceToTarget - rangeMid) * 0.1f;

                // HP が低いほど控えめにスコアを下げる
                if (hpRatio < 0.5f) score *= (0.5f + hpRatio);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = p;
                }
            }

            return best;
        }
    }
}
