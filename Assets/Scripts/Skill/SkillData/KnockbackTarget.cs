namespace Skill.SkillData
{
    /// <summary>
    /// ノックバックの対象設定
    /// </summary>
    public enum KnockbackTarget
    {
        /// <summary>敵のみノックバック</summary>
        Enemy,
        /// <summary>味方のみノックバック（自爆系スキル等）</summary>
        Player,
        /// <summary>全対象にノックバック</summary>
        Both
    }
}