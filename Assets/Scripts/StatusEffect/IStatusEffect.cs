using Character.Interfaces;

namespace StatusEffect
{
    /// <summary>
    /// 状態異常の基本インターフェース
    /// 新しい状態異常を追加する場合はこのインターフェースを実装する
    /// </summary>
    public interface IStatusEffect
    {
        /// <summary>状態異常の識別名</summary>
        string Name { get; }

        /// <summary>状態異常の種類</summary>
        StatusEffectType Type { get; }

        /// <summary>残り時間（秒）</summary>
        float RemainingTime { get; }

        /// <summary>効果が終了したか</summary>
        bool IsExpired { get; }

        /// <summary>スタック可能か</summary>
        bool IsStackable { get; }

        /// <summary>現在のスタック数</summary>
        int StackCount { get; }

        /// <summary>
        /// 効果を適用する（付与時に1回呼ばれる）
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        void Apply(ICharacterStats target);

        /// <summary>
        /// 毎フレームの更新処理
        /// </summary>
        /// <param name="deltaTime">経過時間</param>
        void Update(float deltaTime);

        /// <summary>
        /// 効果を解除する（終了時に1回呼ばれる）
        /// </summary>
        /// <param name="target">対象キャラクター</param>
        void Remove(ICharacterStats target);

        /// <summary>
        /// スタックを追加する
        /// </summary>
        /// <param name="count">追加するスタック数</param>
        void AddStack(int count = 1);

        /// <summary>
        /// 効果時間を延長する
        /// </summary>
        /// <param name="duration">延長時間</param>
        void ExtendDuration(float duration);
    }

    /// <summary>
    /// 状態異常の種類
    /// </summary>
    public enum StatusEffectType
    {
        /// <summary>行動阻害系（昏睡、凍結など）</summary>
        Disable,

        /// <summary>継続ダメージ系（出血、毒など）</summary>
        DamageOverTime,

        /// <summary>継続回復系</summary>
        HealOverTime,

        /// <summary>デバフ系（憂鬱、不安など）</summary>
        Debuff,

        /// <summary>バフ系</summary>
        Buff
    }

    /// <summary>
    /// スキルが付与する状態異常の種別（スキルデータで指定する具体的なエフェクト）
    /// </summary>
    public enum StatusEffectKind
    {
        None,
        Stun,
        Depression,
        Bleeding,
        Anxiety
    }
}