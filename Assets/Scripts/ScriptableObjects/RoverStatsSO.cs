// Assets/Scripts/Player/Data/RoverStatsSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "RoverStats", menuName = "Opportunity/Rover Stats")]
public class RoverStatsSO : ScriptableObject
{
    [Header("Run")]
    public float maxRunSpeed      = 12f;
    public float groundAcceleration  = 40f;   // llega a max en ~0.30 s
    public float groundDeceleration  = 63f;   // frena en ~0.19 s
    [Range(0f, 1f)]
    public float airControlFactor = 0.65f;    // GDD: 65% en aire

    [Header("Jump")]
    public float jumpForce           = 22f;   // alcanza 6.4 u
    [Range(0f, 1f)]
    public float jumpCutMultiplier   = 0.5f;  // salto corto ~2.4 u
    public float coyoteTime          = 0.12f;
    public float jumpBufferTime      = 0.10f;

    [Header("Fall")]
    public float maxFallSpeed        = -26f;  // velocidad terminal GDD

    [Header("Dash")]
    public float dashSpeed           = 28f;
    public float dashDuration        = 0.50f;
    public float dashCooldown        = 0.80f;
    public float dashSleepTime       = 0.028f; // freeze inicial de frames

    [Header("Wall")]
    public float wallSlideSpeed      = -2f;   // deslizamiento lento
    public float wallJumpForceX      = 14f;
    public float wallJumpForceY      = 18f;
    public float wallJumpInputLock   = 0.15f; // bloqueo input horizontal

    [Header("Physics")]
    public float gravityScale        = 20f;   // GDD: -20 (se aplica negado)
    public float fallGravityMultiplier = 1.4f; // caída más pesada que subida
}