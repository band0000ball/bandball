using Unity.Mathematics;
using UnityEngine;

namespace Commons
{
    public class Movement : MonoBehaviour
    {
        [SerializeField] public bool PreventAirAccelerationAgainstUngroundedHits;
        [SerializeField] public float RotationSharpness;
        [SerializeField] public float AirAcceleration;
        [SerializeField] public float AirMaxSpeed;
        [SerializeField] public float AirDrag;
        [SerializeField] public int MaxAirJumps;
        [SerializeField] public float JumpSpeed;
        [SerializeField] public float GroundedMovementSharpness;
        [SerializeField] public float GroundMaxSpeed;
        [SerializeField] public float3 Gravity;
    }
}