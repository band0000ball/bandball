using System;
using UnityEngine;

namespace Monster
{
    /// <summary>
    /// ボスのフェーズ定義。EnemyAIData.bossPhases に配列で保持する。
    /// hpThreshold の降順（高 → 低）で記述すること。
    /// </summary>
    [Serializable]
    public class BossPhase
    {
        public string phaseName = "Phase";

        /// <summary>このフェーズに移行する HP 割合（例: 0.7 = HP70%以下）</summary>
        [Range(0f, 1f)] public float hpThreshold = 0.7f;

        /// <summary>攻撃力倍率</summary>
        public float attackMultiplier = 1f;

        /// <summary>移動速度倍率</summary>
        public float speedMultiplier = 1f;

        /// <summary>フェーズ移行時の無敵時間（秒）</summary>
        public float transitionInvincibleDuration = 2.5f;

        /// <summary>フェーズ固有の追加攻撃パターン</summary>
        public AttackPattern[] additionalPatterns = new AttackPattern[0];
    }
}
