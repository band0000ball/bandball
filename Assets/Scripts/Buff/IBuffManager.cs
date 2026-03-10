namespace Buff
{
    /// <summary>
    /// バフマネージャーのインターフェース
    /// </summary>
    public interface IBuffManager
    {
        /// <summary>
        /// バフの値を取得する
        /// </summary>
        float GetBuffValue(BuffType buffType);

        /// <summary>
        /// バフを追加する
        /// </summary>
        void Add(BuffType buffType, float buffValue, float buffTime);

        /// <summary>
        /// 指定タイプのバフのみを削除する（デバフは残す）
        /// </summary>
        void RemoveBuffByType(BuffType buffType);

        /// <summary>
        /// 指定タイプのデバフのみを削除する（バフは残す）
        /// </summary>
        void RemoveDeBuffByType(BuffType buffType);

        /// <summary>
        /// 指定タイプのバフ/デバフを全て削除する
        /// </summary>
        void RemoveByType(BuffType buffType);

        /// <summary>
        /// 全てのバフ/デバフを削除する
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// 指定タイプのバフの残り時間を減少させる
        /// </summary>
        void DecTimeBuffByType(BuffType buffType, float time);

        /// <summary>
        /// 指定タイプのバフの残り時間を増加させる
        /// </summary>
        void IncTimeBuffByType(BuffType buffType, float time);

        /// <summary>
        /// 指定タイプのバフの値を減少させる
        /// </summary>
        void DecValueBuffByType(BuffType buffType, float value);

        /// <summary>
        /// バフの状態を更新する（毎フレーム呼び出し）
        /// </summary>
        void Update(float deltaTime);
    }
}