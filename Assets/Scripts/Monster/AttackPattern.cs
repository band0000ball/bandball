using System;

namespace Monster
{
    /// <summary>
    /// 敵の攻撃パターン定義。EnemyAIData に配列で保持する。
    /// </summary>
    [Serializable]
    public class AttackPattern
    {
        /// <summary>パターン識別名（クールダウン管理キーとして使用）</summary>
        public string patternName = "Attack";

        /// <summary>攻撃可能な最小距離</summary>
        public float minRange = 0f;

        /// <summary>攻撃可能な最大距離</summary>
        public float maxRange = 3f;

        /// <summary>基本優先スコア（ActionSelector が加減算する）</summary>
        public float baseScore = 1f;

        /// <summary>個別クールダウン（秒）</summary>
        public float cooldown = 2f;

        /// <summary>
        /// 使用するスキルスロット番号。
        /// -1 の場合は通常攻撃（attack=true を設定）。
        /// </summary>
        public int skillSlot = -1;
    }
}
