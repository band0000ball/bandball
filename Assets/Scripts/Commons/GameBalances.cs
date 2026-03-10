namespace Commons
{
    /// <summary>
    /// ゲームバランスに関する定数を一元管理するクラス
    /// 数値調整はこのファイルで行う
    /// </summary>
    public static class GameBalances
    {
        #region ダメージ計算係数

        /// <summary>
        /// 基本ダメージ倍率（攻撃力の50%が基本ダメージ）
        /// </summary>
        public const float DAMAGE_BASE_MULTIPLIER = 0.5f;

        /// <summary>
        /// 防御力による減算係数（防御力の25%分ダメージ軽減）
        /// </summary>
        public const float DEFENSE_REDUCTION_MULTIPLIER = 0.25f;

        /// <summary>
        /// シールドダメージ係数（シールドへのダメージは25%）
        /// </summary>
        public const float SHIELD_DAMAGE_MULTIPLIER = 0.25f;

        /// <summary>
        /// 属性ダメージ係数（属性差分の50%が追加ダメージ）
        /// </summary>
        public const float ATTRIBUTE_DAMAGE_MULTIPLIER = 0.5f;

        /// <summary>
        /// ランダムダメージ変動係数（luck値による変動幅）
        /// </summary>
        public const float RANDOM_DAMAGE_MULTIPLIER = 0.01f;

        /// <summary>
        /// クリティカルダメージ係数（クリティカルダメージ値の10%を倍率に加算）
        /// </summary>
        public const float CRITICAL_DAMAGE_MULTIPLIER = 0.1f;

        #endregion

        #region 継続ダメージ/回復

        /// <summary>
        /// 継続ダメージの基本倍率（攻撃力の2%が継続ダメージ/秒）
        /// </summary>
        public const float DOT_BASE_MULTIPLIER = 0.02f;

        #endregion

        #region 移動パラメータ

        /// <summary>
        /// デフォルト移動速度
        /// </summary>
        public const float DEFAULT_MOVEMENT_SPEED = 13f;

        /// <summary>
        /// しゃがみ時の移動速度倍率
        /// </summary>
        public const float CROUCH_SPEED_MULTIPLIER = 1f;

        /// <summary>
        /// 移動入力の最小閾値（これ以下の入力は無視）
        /// </summary>
        public const float MOVEMENT_INPUT_THRESHOLD = 0.01f;

        /// <summary>
        /// 加速時の補間係数
        /// </summary>
        public const float MOVEMENT_ACCELERATION = 0.2f;

        /// <summary>
        /// 減速時の補間係数
        /// </summary>
        public const float MOVEMENT_DECELERATION = 0.1f;

        #endregion

        #region ジャンプパラメータ

        /// <summary>
        /// 基本ジャンプ速度
        /// </summary>
        public const float JUMP_VELOCITY = 2.4f;

        /// <summary>
        /// 落下時の重力倍率
        /// </summary>
        public const float FALL_GRAVITY_MULTIPLIER = 1.7f;

        /// <summary>
        /// ジャンプボタン長押し時の重力倍率（低いほど高くジャンプ）
        /// </summary>
        public const float HOLD_JUMP_GRAVITY_MULTIPLIER = 5f;

        /// <summary>
        /// コヨーテタイム用のジャンプ倍率（デフォルト）
        /// </summary>
        public const float COYOTE_JUMP_MULTIPLIER = 1f;

        #endregion

        #region 地形・物理パラメータ

        /// <summary>
        /// 壁との摩擦係数
        /// </summary>
        public const float WALL_FRICTION = 0.839f;

        /// <summary>
        /// 接地判定の距離閾値
        /// </summary>
        public const float GROUND_CHECK_THRESHOLD = 0.05f;

        /// <summary>
        /// 登れる坂の最大角度
        /// </summary>
        public const float MAX_CLIMBABLE_SLOPE_ANGLE = 53.6f;

        /// <summary>
        /// 基本重力倍率
        /// </summary>
        public const float GRAVITY_MULTIPLIER = 6f;

        /// <summary>
        /// 法線変化時の重力倍率
        /// </summary>
        public const float GRAVITY_MULTIPLIER_ON_SLIDE_CHANGE = 3f;

        /// <summary>
        /// 登れない坂での重力倍率
        /// </summary>
        public const float GRAVITY_MULTIPLIER_UNCLIMBABLE_SLOPE = 30f;

        #endregion

        #region エフェクト・演出

        /// <summary>
        /// ヒットストップの持続時間（秒）
        /// </summary>
        public const float HIT_STOP_DURATION = 0.23f;

        /// <summary>
        /// エフェクト破棄までのデフォルト遅延（秒）
        /// </summary>
        public const float EFFECT_DESTROY_DELAY = 1f;

        /// <summary>
        /// メインエフェクト破棄時の追加遅延（秒）
        /// </summary>
        public const float MAIN_EFFECT_DESTROY_ADDITIONAL_DELAY = 0.5f;

        #endregion

        #region 状態異常（将来実装用）

        /// <summary>
        /// 昏睡の基本持続時間（秒）
        /// </summary>
        public const float STUN_BASE_DURATION = 2.0f;

        /// <summary>
        /// 出血の基本ダメージ（/秒）
        /// </summary>
        public const float BLEEDING_DAMAGE_PER_SECOND = 5.0f;

        /// <summary>
        /// 憂鬱のスキル遅延倍率
        /// </summary>
        public const float DEPRESSION_SKILL_DELAY_MULTIPLIER = 1.5f;

        /// <summary>
        /// 不安のスタミナ回復停止時間（秒）
        /// </summary>
        public const float ANXIETY_STAMINA_BLOCK_DURATION = 3.0f;

        #endregion
    }
}
