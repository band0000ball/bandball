namespace Level
{
    /// <summary>
    /// アビリティノードの効果種別。
    /// UnlockSkill の場合は EffectSkillGuid にスキルGUIDを格納する。
    /// </summary>
    public enum AbilityEffectType
    {
        MaxHealth,
        MaxStamina,
        AttackPower,
        DefensePower,
        MoveSpeed,
        BaseAttributePower,
        BaseResistancePower,
        UnlockSkill,
    }

    /// <summary>
    /// CSV から読み込まれるアビリティノードの静的定義。
    /// ランタイムでは変更されない（習得状態は AbilityTreeManager が管理）。
    /// </summary>
    public class AbilityNodeDef
    {
        /// <summary>一意なノードID</summary>
        public string NodeId;

        public string Name;

        /// <summary>習得に必要なAP</summary>
        public int ApCost;

        public AbilityEffectType EffectType;

        /// <summary>ステータス系ノードの増加量</summary>
        public float EffectValue;

        /// <summary>UnlockSkill ノードのスキルGUID</summary>
        public string EffectSkillGuid;

        /// <summary>前提ノードIDリスト（全て習得済みである必要がある）</summary>
        public string[] Prerequisites;

        /// <summary>ツリー上の段（表示に使用）</summary>
        public int Tier;

        /// <summary>カテゴリノードの場合に設定。空文字なら共通または個人ノード。</summary>
        public string CategoryId;

        /// <summary>キャラ固有ノードの場合に設定。0なら共通またはカテゴリノード。</summary>
        public int CharacterId;
    }
}
