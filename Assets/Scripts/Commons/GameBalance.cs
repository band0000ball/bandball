namespace Commons
{
    public class GameBalance
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

        #region ジャンプ詳細パラメータ

        /// <summary>
        /// ジャンプクールダウン時間（秒）
        /// </summary>
        public const float JUMP_COOLDOWN_TIME = 0.75f;

        /// <summary>
        /// ジャンプ前の待機時間（秒）
        /// </summary>
        public const float JUMP_DELAY_TIME = 0.25f;

        /// <summary>
        /// ジャンプ力ボーナス係数
        /// </summary>
        public const float JUMP_FORCE_BONUS_MULTIPLIER = 0.1f;

        /// <summary>
        /// 上昇判定の速度閾値
        /// </summary>
        public const float RISING_VELOCITY_THRESHOLD = 0.1f;

        #endregion

        #region 位置補正

        /// <summary>
        /// Z軸位置補正の閾値
        /// </summary>
        public const float Z_AXIS_CORRECTION_THRESHOLD = 1f;

        #endregion

        #region ノックバック

        /// <summary>
        /// ノックバック力のスケーリング係数
        /// </summary>
        public const float KNOCKBACK_FORCE_MULTIPLIER = 5f;

        /// <summary>
        /// ノックバック中の入力制限時間（秒）
        /// </summary>
        public const float KNOCKBACK_INPUT_LOCK_DURATION = 0.3f;

        #endregion

        #region ダメージ演出

        /// <summary>
        /// ダメージ/回復数値のY位置オフセット
        /// </summary>
        public const float DAMAGE_NUMBER_Y_OFFSET = 30f;

        /// <summary>
        /// 死亡後オブジェクト破棄遅延（秒）
        /// </summary>
        public const float DEATH_DESTROY_DELAY = 3f;

        /// <summary>
        /// ダメージシェイク強度
        /// </summary>
        public const float DAMAGE_SHAKE_STRENGTH = 0.15f;

        /// <summary>
        /// ダメージシェイク振動回数
        /// </summary>
        public const int DAMAGE_SHAKE_VIBRATO = 25;

        /// <summary>
        /// ノックバック距離
        /// </summary>
        public const float KNOCKBACK_DISTANCE = 1f;

        /// <summary>
        /// ノックバック演出時間（秒）
        /// </summary>
        public const float KNOCKBACK_DURATION = 0.2f;

        /// <summary>
        /// ダメージモーションのブレンド開始位置（正規化時間）
        /// </summary>
        public const float DAMAGE_MOTION_BLEND_START = 0.2f;

        #endregion

        #region ガード

        /// <summary>
        /// ガードアニメーション停止タイミング（正規化時間）
        /// </summary>
        public const float GUARD_ANIMATION_STOP_TIME = 0.5f;

        /// <summary>
        /// デフォルトガードクールダウン（秒）
        /// </summary>
        public const float DEFAULT_GUARD_COOLDOWN = 5f;

        /// <summary>
        /// ガードアイコンパンチスケール
        /// </summary>
        public const float GUARD_ICON_PUNCH_SCALE = 1.0f;

        /// <summary>
        /// ガードアイコンアニメーション時間（秒）
        /// </summary>
        public const float GUARD_ICON_ANIMATION_DURATION = 0.2f;

        #endregion

        #region パリィ

        /// <summary>
        /// パリィ判定ウィンドウ（秒）。ガード入力から この時間内に攻撃を受けるとパリィ成功。
        /// </summary>
        public const float PARRY_WINDOW = 0.2f;

        /// <summary>
        /// パリィ成功時のスタミナ回復量
        /// </summary>
        public const float PARRY_STAMINA_RECOVERY = 20f;

        /// <summary>
        /// パリィ成功後の反撃チャンスウィンドウ（秒）
        /// </summary>
        public const float PARRY_COUNTER_WINDOW = 1.5f;

        #endregion

        #region ローリング

        /// <summary>
        /// ローリング持続時間（秒）
        /// </summary>
        public const float ROLLING_DURATION = 0.35f;

        /// <summary>
        /// 無敵時間（秒）。ローリング開始からこの時間が無敵判定。
        /// </summary>
        public const float ROLLING_INVINCIBLE_TIME = 0.25f;

        /// <summary>
        /// ローリングのクールダウン（秒）
        /// </summary>
        public const float ROLLING_COOLDOWN = 1.2f;

        /// <summary>
        /// ローリング速度倍率（通常移動速度に乗じる）
        /// </summary>
        public const float ROLLING_SPEED_MULTIPLIER = 2.5f;

        /// <summary>
        /// ダブルタップ判定ウィンドウ（秒）
        /// </summary>
        public const float ROLLING_DOUBLE_TAP_WINDOW = 0.3f;

        #endregion

        #region チャージ攻撃

        /// <summary>チャージ開始までの攻撃ボタン長押し時間（秒）</summary>
        public const float CHARGE_START_THRESHOLD = 0.5f;

        /// <summary>ChargeState突入後、Stage1に達するまでの時間（秒）</summary>
        public const float CHARGE_TIME_STAGE1 = 0.5f;

        /// <summary>ChargeState突入後、Stage2に達するまでの時間（秒）</summary>
        public const float CHARGE_TIME_STAGE2 = 1.0f;

        /// <summary>ChargeState突入後、Stage3（最大）に達するまでの時間（秒）</summary>
        public const float CHARGE_TIME_STAGE3 = 1.8f;

        /// <summary>
        /// チャージ段階ごとのダメージ倍率。index = チャージ段階（0=即離し, 1〜3）。
        /// </summary>
        public static readonly float[] CHARGE_DAMAGE_MULTIPLIERS = { 1.0f, 1.5f, 2.5f, 4.0f };

        #endregion

        #region 難易度・死亡ペナルティ

        /// <summary>ノーマル難易度: 死亡時通貨ロスト率（10%）</summary>
        public const float NORMAL_CURRENCY_LOSS_RATE = 0.1f;

        /// <summary>ハード難易度: 死亡時通貨ロスト率（50%）</summary>
        public const float HARD_CURRENCY_LOSS_RATE = 0.5f;

        /// <summary>ウルトラ難易度: 死亡時通貨ロスト率（100%）</summary>
        public const float ULTRA_CURRENCY_LOSS_RATE = 1.0f;

        /// <summary>アイテム/装備の回収制限時間（秒）: 10分</summary>
        public const float ITEM_RECOVERY_TIME_LIMIT = 600f;

        /// <summary>キャラクターの回収制限時間（秒）: 30分</summary>
        public const float CHARACTER_RECOVERY_TIME_LIMIT = 1800f;

        /// <summary>ハード難易度: 装備ドロップ確率（レアリティ係数を掛ける）</summary>
        public const float HARD_EQUIP_DROP_BASE_RATE = 0.3f;

        /// <summary>アイテム保護コスト: レアリティ別（index = CharacterRarity）</summary>
        public static readonly int[] ITEM_PROTECTION_COSTS = { 10, 50, 200, 1000, 5000, 10000 };

        #endregion

        #region アビリティツリー

        /// <summary>
        /// アビリティリセット時のゴールドレート。消費AP × このレート = 必要ゴールド。
        /// </summary>
        public const int ABILITY_RESET_GOLD_RATE = 10;

        #endregion

        #region UI

        /// <summary>
        /// マウスポイントのZ距離
        /// </summary>
        public const float MOUSE_POINT_Z_DISTANCE = 40f;

        /// <summary>
        /// コライダー高さ表示倍率
        /// </summary>
        public const float COLLIDER_HEIGHT_DISPLAY_MULTIPLIER = 2f;

        #endregion

        #region スキル発動

        /// <summary>
        /// スキル発動速度の基本値（Control値が0の時の速度倍率）
        /// </summary>
        public const float INVOKE_BASE_SPEED = 0.9f;

        /// <summary>
        /// Control値によるスキル発動速度の加算係数
        /// 最終速度 = INVOKE_BASE_SPEED + INVOKE_CONTROL_MULTIPLIER * Control
        /// </summary>
        public const float INVOKE_CONTROL_MULTIPLIER = 0.1f;

        /// <summary>
        /// 連続スキル発動時の遅延間隔（秒）
        /// </summary>
        public const float INVOKE_SKILL_DELAY_INTERVAL = 0.1f;

        #endregion

        #region コンボシステム

        /// <summary>
        /// コンボウィンドウの持続時間（秒）
        /// </summary>
        public const float COMBO_WINDOW_TIME = 1.0f;

        /// <summary>
        /// コンボウィンドウが開く攻撃アニメーションの正規化時間
        /// </summary>
        public const float COMBO_WINDOW_OPEN_NORMALIZED_TIME = 0.4f;

        /// <summary>
        /// コンボごとのダメージ倍率（インデックス = コンボ数 - 1）
        /// </summary>
        public static readonly float[] COMBO_DAMAGE_MULTIPLIERS =
            { 1.0f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 2.5f };

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

        /// <summary>
        /// 状態異常の発生時間計算乗算定数
        /// 発生時間 = (攻撃側属性値 - 防御側抵抗値) × この定数
        /// </summary>
        public const float STATUS_EFFECT_DURATION_MULTIPLIER = 1.0f;

        #endregion
    }
}