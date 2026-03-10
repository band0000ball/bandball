using UnityEngine;

namespace Character.Interfaces
{
    /// <summary>
    /// キャラクターの移動を管理するインターフェース
    /// 物理演算とジャンプ処理を担当
    /// </summary>
    public interface ICharacterMovement
    {
        #region State Properties

        /// <summary>接地しているか</summary>
        bool IsGrounded { get; }

        /// <summary>ジャンプ中か</summary>
        bool IsJumping { get; }

        /// <summary>しゃがみ中か</summary>
        bool IsCrouching { get; }

        /// <summary>壁に触れているか</summary>
        bool IsTouchingWall { get; }

        /// <summary>坂に触れているか</summary>
        bool IsTouchingSlope { get; }

        /// <summary>段差に触れているか</summary>
        bool IsTouchingStep { get; }

        /// <summary>現在の移動速度</summary>
        Vector3 Velocity { get; }

        /// <summary>Y軸回転角度</summary>
        float RotateY { get; }

        /// <summary>元のコライダー高さ</summary>
        float OriginalColliderHeight { get; }

        /// <summary>登れる最大坂角度</summary>
        float MaxClimbableSlopeAngle { get; }

        /// <summary>現在の地面角度</summary>
        float CurrentSurfaceAngle { get; }

        #endregion

        #region Movement Methods

        /// <summary>
        /// 物理更新（FixedUpdateで呼び出す）
        /// </summary>
        /// <param name="deltaTime">固定デルタタイム</param>
        void UpdatePhysics(float deltaTime);

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="direction">移動方向（-1〜1）</param>
        /// <param name="speedMultiplier">速度倍率</param>
        void Move(float direction, float speedMultiplier = 1f);

        /// <summary>
        /// ジャンプ開始
        /// </summary>
        /// <returns>ジャンプが成功したか</returns>
        bool Jump();

        /// <summary>
        /// ジャンプボタンを離した時の処理
        /// </summary>
        void ReleaseJump();

        /// <summary>
        /// しゃがみ状態の設定
        /// </summary>
        /// <param name="crouch">しゃがむか</param>
        void SetCrouch(bool crouch);

        /// <summary>
        /// 移動を停止する
        /// </summary>
        void Stop();

        #endregion

        #region Configuration

        /// <summary>
        /// 移動パラメータを設定する
        /// </summary>
        void SetMovementParameters(
            float moveSpeed,
            float jumpForce,
            float gravityMultiplier);

        #endregion
    }
}
