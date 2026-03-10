using System;
using System.Threading;
using System.Threading.Tasks;
using Character.Interfaces;
using Commons;
using UnityEngine;

namespace Character
{
    /// <summary>
    /// キャラクターの移動を管理するコンポーネント
    /// CharacterControlから分離された物理演算とジャンプ処理を担当
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterMovement : MonoBehaviour, ICharacterMovement
    {
        #region Serialized Fields

        [Header("Movement")]
        [SerializeField] private float _moveSpeed = GameBalance.DEFAULT_MOVEMENT_SPEED;
        [SerializeField] private float _movementThreshold = GameBalance.MOVEMENT_INPUT_THRESHOLD;
        [SerializeField] private float _acceleration = GameBalance.MOVEMENT_ACCELERATION;
        [SerializeField] private float _deceleration = GameBalance.MOVEMENT_DECELERATION;
        [SerializeField] private bool _smoothMovement;

        [Header("Jump")]
        [SerializeField] private float _jumpForce = GameBalance.JUMP_VELOCITY;
        [SerializeField] private bool _canLongJump = true;

        [Header("Gravity")]
        [SerializeField] private float _gravityMultiplier = GameBalance.GRAVITY_MULTIPLIER;
        [SerializeField] private float _fallMultiplier = GameBalance.FALL_GRAVITY_MULTIPLIER;
        [SerializeField] private float _holdJumpMultiplier = GameBalance.HOLD_JUMP_GRAVITY_MULTIPLIER;

        [Header("Ground Check")]
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _groundCheckThreshold = GameBalance.GROUND_CHECK_THRESHOLD;
        [SerializeField] private float _maxClimbableSlopeAngle = GameBalance.MAX_CLIMBABLE_SLOPE_ANGLE;

        [Header("Wall")]
        [SerializeField] private float _wallFriction = GameBalance.WALL_FRICTION;

        #endregion

        #region Private Fields

        private CharacterControl _control;
        private Animator _animator;
        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;

        private bool _isGrounded;
        private bool _isJumping;
        private bool _isTouchingWall;
        private bool _isTouchingSlope;
        private bool _isTouchingStep;
        private bool _jumpHold;

        private int _jumpCount;
        private float _jumpCooldownTimer;
        private float _coyoteJumpMultiplier = GameBalance.COYOTE_JUMP_MULTIPLIER;

        private Vector3 _currentVelocity;
        private Vector3 _groundNormal;
        private float _currentSurfaceAngle;
        private float _rotateY;

        private float _colliderHeight;
        private float _colliderRadius;

        // Animator hashes
        private static readonly int WalkHash = Animator.StringToHash("Walk");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        #endregion

        #region ICharacterMovement Properties

        public bool IsGrounded => _isGrounded;
        public bool IsJumping => _isJumping;
        public bool IsCrouching => false; // Managed by CharacterCombat
        public bool IsTouchingWall => _isTouchingWall;
        public bool IsTouchingSlope => _isTouchingSlope;
        public bool IsTouchingStep => _isTouchingStep;
        public Vector3 Velocity => _rigidbody.linearVelocity;
        public float RotateY => _rotateY;
        public float OriginalColliderHeight => _colliderHeight;
        public float MaxClimbableSlopeAngle => _maxClimbableSlopeAngle;
        public float CurrentSurfaceAngle => _currentSurfaceAngle;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            _colliderHeight = _collider.height;
            _colliderRadius = _collider.radius;

            if (_groundMask == 0)
            {
                _groundMask = LayerMask.GetMask("Ground");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// CharacterControlから呼び出される初期化
        /// </summary>
        public void Initialize(CharacterControl control, Animator animator)
        {
            _control = control;
            _animator = animator;
            _jumpCount = 0;
        }

        #endregion

        #region Public Methods - Called by CharacterControl

        /// <summary>
        /// 移動処理（FixedUpdateから呼び出し）
        /// 行動可否はStateMachineがaxisInput/jumpInputに反映済みのものを受け取る
        /// </summary>
        public void ProcessMovement(Vector2 axisInput, bool jumpInput)
        {
            CheckGrounded();
            MoveWalk(axisInput, jumpInput);
            MoveJump(jumpInput);
        }

        /// <summary>
        /// 重力処理（FixedUpdateから呼び出し）
        /// </summary>
        public void ProcessGravity()
        {
            ApplyGravity(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Z軸位置の補正
        /// </summary>
        public void FixZAxis()
        {
            if (Math.Abs(transform.position.z) > GameBalance.Z_AXIS_CORRECTION_THRESHOLD)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            }
        }

        /// <summary>
        /// ジャンプクールダウンの減少
        /// </summary>
        public void DecreaseJumpCooldown(float deltaTime)
        {
            if (_jumpCooldownTimer > 0)
                _jumpCooldownTimer -= deltaTime;
            else
                _jumpCooldownTimer = 0f;
        }

        /// <summary>
        /// 移動停止
        /// </summary>
        public void StopMovement()
        {
            _rigidbody.linearVelocity = Vector3.zero;
        }

        /// <summary>
        /// 位置取得
        /// </summary>
        public Vector3 GetPosition()
        {
            return _collider.bounds.center;
        }

        /// <summary>
        /// 高さ取得
        /// </summary>
        public float GetHeight()
        {
            return _collider.height;
        }

        /// <summary>
        /// Rigidbody位置との差分X
        /// </summary>
        public float GetRigidPositionDiffX(Vector3 position)
        {
            return (_rigidbody.position - position).x;
        }

        #endregion

        #region ICharacterMovement Methods (Legacy interface support)

        public void UpdatePhysics(float deltaTime)
        {
            CheckGrounded();
            DecreaseJumpCooldown(deltaTime);
            ApplyGravity(deltaTime);
            FixZAxis();
        }

        public void Move(float direction, float speedMultiplier = 1f)
        {
            if (Mathf.Abs(direction) < _movementThreshold)
            {
                ApplyMovement(Vector3.zero, _deceleration);
                return;
            }

            Vector3 moveDirection = new Vector3(direction, 0f, 0f).normalized;
            float speed = _moveSpeed * speedMultiplier * _colliderRadius;

            ApplyMovement(moveDirection * speed, _acceleration);
        }

        public bool Jump()
        {
            if (_control == null) return false;
            if (_jumpCount >= _control.GetJumpTime()) return false;
            if (_jumpCooldownTimer > 0) return false;

            _jumpCount++;
            _ = JumpAsync(destroyCancellationToken);
            _jumpHold = true;
            _isJumping = true;

            return true;
        }

        public void ReleaseJump()
        {
            _jumpHold = false;
        }

        public void SetCrouch(bool crouch)
        {
            // Managed by CharacterCombat
        }

        public void Stop()
        {
            _rigidbody.linearVelocity = new Vector3(0, _rigidbody.linearVelocity.y, 0);
        }

        public void SetMovementParameters(float moveSpeed, float jumpForce, float gravityMultiplier)
        {
            _moveSpeed = moveSpeed;
            _jumpForce = jumpForce;
            _gravityMultiplier = gravityMultiplier;
        }

        #endregion

        #region Private Methods - Movement

        private void MoveWalk(Vector2 axisInput, bool jumpInput)
        {
            if (_control == null || _animator == null) return;

            float delta = Time.fixedDeltaTime;

            Vector3 MakeVelocity(Vector3 move, float dampSpeed)
            {
                return _smoothMovement
                    ? Vector3.SmoothDamp(_rigidbody.linearVelocity, _rigidbody.linearVelocity + move, ref _currentVelocity, dampSpeed)
                    : new Vector3(move.x * delta * dampSpeed, _rigidbody.linearVelocity.y, _rigidbody.linearVelocity.z);
            }

            if (Math.Abs(axisInput.x) > _movementThreshold)
            {
                Vector3 forward = new Vector3(axisInput.x, 0f, 0f).normalized;
                float moveSpeed = _moveSpeed;

                if (jumpInput)
                    moveSpeed += _control.GetJumpSpeed();
                else
                    moveSpeed += _control.GetMoveSpeed();

                moveSpeed *= _colliderRadius;
                _rigidbody.linearVelocity = MakeVelocity(forward * moveSpeed, _acceleration);

                float faceAngle = _control.MetaStatus.isFacingRight ? 0f : 180f;
                _control.MeshCharacter.transform.rotation = Quaternion.AngleAxis(forward.x > 0f ? faceAngle : faceAngle + 180f, Vector3.up);
                _rotateY = _control.MeshCharacter.transform.rotation.eulerAngles.y;

                if (jumpInput) return;

                _animator.SetBool(WalkHash, true);
                if (!float.IsNaN(_control.PrevRotate))
                    _animator.SetFloat(SpeedHash, forward.x >= 0f ? 1 : -1);
                else
                    _animator.SetFloat(SpeedHash, 1);
            }
            else
            {
                _rigidbody.linearVelocity = MakeVelocity(Vector3.zero, _deceleration);
                if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    _animator.SetBool(WalkHash, false);
                }
            }
        }

        private void MoveJump(bool jumpInput)
        {
            if (_control == null || _animator == null) return;

            AnimatorStateInfo currentAnimator = _animator.GetCurrentAnimatorStateInfo(0);

            if (jumpInput && _jumpCount < _control.GetJumpTime() && _jumpCooldownTimer <= 0)
            {
                if (!currentAnimator.IsName("Jump") || currentAnimator.normalizedTime >= 1f)
                {
                    _jumpCount++;
                    _ = JumpAsync(destroyCancellationToken);
                    _jumpHold = true;
                }
            }

            if (_isGrounded && _jumpCooldownTimer <= 0)
                _jumpCount = 0;

            if (((!currentAnimator.IsName("Jump") || currentAnimator.normalizedTime >= 1.0f) && _isGrounded) || !jumpInput)
            {
                _jumpHold = false;
            }

            UpdateCoyoteMultiplier();
        }

        private async ValueTask JumpAsync(CancellationToken token)
        {
            if (_animator == null || _control == null) return;

            _animator.SetTrigger(JumpHash);
            _jumpCooldownTimer = GameBalance.JUMP_COOLDOWN_TIME;
            await Awaitable.WaitForSecondsAsync(GameBalance.JUMP_DELAY_TIME, token);

            float jumpForce = _jumpForce + GameBalance.JUMP_FORCE_BONUS_MULTIPLIER * _control.GetJumpForce();
            _rigidbody.AddForce(Vector3.up * (_colliderHeight * jumpForce), ForceMode.Impulse);
        }

        #endregion

        #region Private Methods - Physics

        private void CheckGrounded()
        {
            _isGrounded = Physics.CheckSphere(_rigidbody.position, _groundCheckThreshold, _groundMask);

            if (_isGrounded && _jumpCooldownTimer <= 0)
            {
                _jumpCount = 0;
                _isJumping = false;
            }
        }

        private void ApplyMovement(Vector3 targetVelocity, float dampSpeed)
        {
            if (_smoothMovement)
            {
                _rigidbody.linearVelocity = Vector3.SmoothDamp(
                    _rigidbody.linearVelocity,
                    _rigidbody.linearVelocity + targetVelocity,
                    ref _currentVelocity,
                    dampSpeed);
            }
            else
            {
                float deltaTime = Time.fixedDeltaTime;
                _rigidbody.linearVelocity = new Vector3(
                    targetVelocity.x * deltaTime * dampSpeed,
                    _rigidbody.linearVelocity.y,
                    _rigidbody.linearVelocity.z);
            }
        }

        private void ApplyGravity(float deltaTime)
        {
            Vector3 gravity = Vector3.down * (_gravityMultiplier * -Physics.gravity.y * _coyoteJumpMultiplier);

            if (_isTouchingWall && _rigidbody.linearVelocity.y < 0)
            {
                gravity *= _wallFriction;
            }

            _rigidbody.linearVelocity += gravity * deltaTime / _rigidbody.mass;
        }

        private void UpdateCoyoteMultiplier()
        {
            if (_rigidbody.linearVelocity.y < 0 && !_isGrounded)
            {
                _coyoteJumpMultiplier = _fallMultiplier;
            }
            else if (_rigidbody.linearVelocity.y > GameBalance.RISING_VELOCITY_THRESHOLD && (_currentSurfaceAngle <= _maxClimbableSlopeAngle || _isTouchingStep))
            {
                if (!_jumpHold || !_canLongJump)
                    _coyoteJumpMultiplier = GameBalance.COYOTE_JUMP_MULTIPLIER;
                else
                    _coyoteJumpMultiplier = 1f / _holdJumpMultiplier;
            }
            else
            {
                _isJumping = false;
                _coyoteJumpMultiplier = GameBalance.COYOTE_JUMP_MULTIPLIER;
            }
        }

        #endregion

        #region Public Utility Methods

        public void SetVelocity(Vector3 velocity)
        {
            _rigidbody.linearVelocity = velocity;
        }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            _rigidbody.AddForce(force, mode);
        }

        /// <summary>
        /// ノックバック力を適用する（X方向のみ）
        /// </summary>
        /// <param name="direction">ノックバックの方向（攻撃元→被弾者）</param>
        /// <param name="power">ノックバック力</param>
        public void ApplyKnockback(Vector3 direction, float power)
        {
            Vector3 knockbackDir = new Vector3(direction.x, 0f, 0f).normalized;
            _rigidbody.AddForce(knockbackDir * (power * GameBalance.KNOCKBACK_FORCE_MULTIPLIER), ForceMode.Impulse);
        }

        /// <summary>
        /// ローリング中の水平速度を毎FixedUpdateで維持する。
        /// CharacterControl.CanMove=false でProcessMovementがXを0にするため、後段で上書きして維持する。
        /// </summary>
        public void MaintainRollVelocity(float direction, float additionalSpeed)
        {
            float rollSpeed = (_moveSpeed + additionalSpeed) * _colliderRadius * GameBalance.ROLLING_SPEED_MULTIPLIER;
            _rigidbody.linearVelocity = new Vector3(direction * rollSpeed, _rigidbody.linearVelocity.y, 0f);
        }

        public float GetColliderHeight()
        {
            return _colliderHeight;
        }

        #endregion
    }
}
