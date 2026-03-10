using Unity.Mathematics;
using UnityEngine;

namespace Commons
{
    public class InputBase : MonoBehaviour
    {
        public bool enableJump;
        
        [HideInInspector] public float2 move;
        [HideInInspector] public float2 look;
        [HideInInspector] public bool jump;
        [HideInInspector] public bool crouch;
        [HideInInspector] public bool attack;
        [HideInInspector] public bool guard;
        [HideInInspector] public bool modeChange;
        [HideInInspector] public bool roll;
        [HideInInspector] public float rollDirection; // 1 = 右, -1 = 左

        [HideInInspector] public bool hasJumped;
        [HideInInspector] public bool skippedFrame;
        

        protected void JumpProcess(float y)
        {
            if (!enableJump) return;
            
            jump = y > 0f;
            if (!jump) return;
            
            hasJumped = true;
            skippedFrame = false;
        }
    }
}