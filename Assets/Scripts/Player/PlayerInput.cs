using System;
using Commons;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInput : InputBase
    {
        private InputSystemActions inputSystem;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction attackAction;
        private InputAction guardAction;
        private InputAction modeChangeAction;
        private CinemachineImpulseSource impulseSource;
        private RectTransform aimCursor;
        private RectTransform canvasRect;
        private Canvas canvas;
        [HideInInspector] public Camera cam;

        // ダブルタップ検知用
        private float _lastTapTime = -1f;
        private float _lastTapDir = 0f;
        private bool _prevHasXInput;

        [Obsolete("Obsolete")]
        void Start()
        {
            cam = Camera.main;
            inputSystem = new InputSystemActions();
            inputSystem.Enable();
        
            moveAction = inputSystem.Player.Move;
            lookAction = inputSystem.Player.Look;
            attackAction = inputSystem.Player.Attack;
            guardAction = inputSystem.Player.Guard;
            modeChangeAction = inputSystem.Player.ModeChange;
            impulseSource = FindObjectOfType<CinemachineImpulseSource>();
            aimCursor = GameObject.Find("Aim").GetComponent<RectTransform>();
            GameObject canvasObj = GameObject.Find("UserUI");
            canvas = canvasObj.GetComponent<Canvas>();
            canvasRect = canvasObj.GetComponent<RectTransform>();
        }

        void Update()
        {
            look = lookAction.ReadValue<Vector2>();
            float x = moveAction.ReadValue<Vector2>().x;
            float y = moveAction.ReadValue<Vector2>().y;
            move = new float2(x, 0f);
            // look = lookAction.ReadValue<Vector2>();
            JumpProcess(y);
            guard = y < 0f;
            attack = attackAction.ReadValue<float>()> 0f;
            // guard = guardAction.ReadValue<float>()> 0f;
            modeChange = modeChangeAction.ReadValue<float>()> 0f;
            roll = false;
            DetectRollInput(x);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out var newPoint);
            aimCursor.anchoredPosition = newPoint;
        }

        /// <summary>
        /// X軸のダブルタップを検知してroll入力を設定する。
        /// 同方向へのキー入力の立ち上がりが ROLLING_DOUBLE_TAP_WINDOW 秒以内に2回あるとroll=trueになる。
        /// </summary>
        private void DetectRollInput(float x)
        {
            bool hasXInput = Mathf.Abs(x) > 0.5f;
            if (hasXInput && !_prevHasXInput)
            {
                float dir = Mathf.Sign(x);
                float now = Time.time;
                if (dir == _lastTapDir && _lastTapTime > 0f && now - _lastTapTime < Commons.GameBalance.ROLLING_DOUBLE_TAP_WINDOW)
                {
                    roll = true;
                    rollDirection = dir;
                    _lastTapTime = -1f;
                    _lastTapDir = 0f;
                }
                else
                {
                    _lastTapDir = dir;
                    _lastTapTime = now;
                }
            }
            _prevHasXInput = hasXInput;
        }
        
        private void FixedUpdate()
        {
            // if (hasJumped && skippedFrame)
            // {
            //     jump = false;
            //     hasJumped = false;
            // }
            //
            // if (!skippedFrame && enableJump) skippedFrame = true;
        }

        private void OnDestroy()
        {
            inputSystem.Disable();
            inputSystem.Dispose();
        }

        public void GenerateImpulse()
        {
            impulseSource.GenerateImpulse();
        }

        public Vector3 WorldToScreenPoint(Vector3 worldPoint)
        {
            return cam.WorldToScreenPoint(worldPoint);
        }

        public Vector3 ScreenToWorldPoint(Vector3 screenPoint)
        {
            return cam.ScreenToWorldPoint(screenPoint);
        }
        
        public float CamDistance => cam.transform.position.z;

        public Vector3 WorldMousePoint(float distance = float.NaN)
        {
            if (float.IsNaN(distance)) distance = -CamDistance;
            return cam.ScreenToWorldPoint(new Vector3(look.x, look.y, distance));
        }
    }
    
    /*public class PlayerInput : MonoBehaviour
    {
        [Header("Input specs")] 
        public UnityEvent changedInputToMouseAndKeyboard;
        public UnityEvent changedInputToGamepad;

        [Header("Enable inputs")] public bool enableJump = true;
        public bool enableCrouch = true;
        public bool enableSprint = true;


        [HideInInspector] public Vector2 axisInput;
        [HideInInspector] public bool jump;
        [HideInInspector] public bool jumpHold;
        [HideInInspector] public bool sprint;
        [HideInInspector] public bool crouch;


        private bool hasJumped = false;
        private bool skippedFrame = false;
        private bool isMouseAndKeyboard = true;
        private bool oldInput = true;

        private InputSystemActions movementActions;

        private void Awake()
        {
            movementActions = new InputSystemActions();

            movementActions.Player.Move.performed += ctx => OnMove(ctx);

            movementActions.Player.Jump.performed += ctx => OnJump();
            movementActions.Player.Jump.canceled += ctx => JumpEnded();

            movementActions.Player.Camera.performed += ctx => OnCamera(ctx);

            movementActions.Player.Sprint.performed += ctx => OnSprint(ctx);
            movementActions.Player.Sprint.canceled += ctx => SprintEnded(ctx);

            movementActions.Player.Crouch.performed += ctx => OnCrouch(ctx);
            movementActions.Player.Crouch.canceled += ctx => CrouchEnded(ctx);
        }


        //DISABLE if using old input system
        private void GetDeviceNew(InputAction.CallbackContext ctx)
        {
            oldInput = isMouseAndKeyboard;

            if (ctx.control.device is Keyboard || ctx.control.device is Mouse) isMouseAndKeyboard = true;
            else isMouseAndKeyboard = false;

            if (oldInput != isMouseAndKeyboard && isMouseAndKeyboard) changedInputToMouseAndKeyboard.Invoke();
            else if (oldInput != isMouseAndKeyboard && !isMouseAndKeyboard) changedInputToGamepad.Invoke();
        }


        #region Actions

        //DISABLE if using old input system
        private void OnMove(InputAction.CallbackContext ctx)
        {
            axisInput = ctx.ReadValue<Vector2>();
            GetDeviceNew(ctx);
        }


        private void OnJump()
        {
            if (enableJump)
            {
                jump = true;
                jumpHold = true;

                hasJumped = true;
                skippedFrame = false;
            }
        }


        private void JumpEnded()
        {
            jump = false;
            jumpHold = false;
        }



        private void FixedUpdate()
        {
            if (hasJumped && skippedFrame)
            {
                jump = false;
                hasJumped = false;
            }

            if (!skippedFrame && enableJump) skippedFrame = true;
        }



        //DISABLE if using old input system
        private void OnCamera(InputAction.CallbackContext ctx)
        {
            GetDeviceNew(ctx);
        }


        //DISABLE if using old input system
        private void OnSprint(InputAction.CallbackContext ctx)
        {
            if (enableSprint) sprint = true;
        }


        //DISABLE if using old input system
        private void SprintEnded(InputAction.CallbackContext ctx)
        {
            sprint = false;
        }


        //DISABLE if using old input system
        private void OnCrouch(InputAction.CallbackContext ctx)
        {
            if (enableCrouch) crouch = true;
        }


        //DISABLE if using old input system
        private void CrouchEnded(InputAction.CallbackContext ctx)
        {
            crouch = false;
        }

        #endregion


        #region Enable / Disable

        //DISABLE if using old input system
        private void OnEnable()
        {
            movementActions.Enable();
        }


        //DISABLE if using old input system
        private void OnDisable()
        {
            movementActions.Disable();
        }

        #endregion
    }*/
}