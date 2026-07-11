namespace MovementRoguelike3D;

public static class Prelude {
    #region Abilities

    public static float MoveStrength => 1.5f;
    public static float AirMoveMultiplier => 0.3f;
    public static float JumpImpulse => 15f;
    public static float SprintMult => 2f;
    public static float WallJumpMultiplier => 1.5f;
    public static float SprintAccelSpeedCap => 9f;
    public static Func<float, float> WallRunningGravity => runSpeed => Mathf.Min(1f, 1f / runSpeed);
    public static Vector3 WallJumpDirection(Player.Player player) => player.GetWallNormal().Normalized() + Vector3.Up;

    #endregion

    #region Input

    public static float MouseSensitivity => .003f;
    public static float MinPitch => -90;
    public static float MaxPitch => 90;

    #endregion

    #region Drag

    public static float GroundDrag => 0.80f;
    public static float AirDrag => 0.95f;

    public static float SprintDrag(float x, float drag) =>
        Mathf.Lerp(1, drag, Mathf.Clamp((x - 2) * 0.05f, 0f, 1f));

    #endregion

    #region Visual

    private static float fov = 90;
    public static event Action<float>? OnFovChange;

    public static float Fov {
        get => fov;
        set {
            fov = value;
            OnFovChange?.Invoke(fov);
        }
    }

    public const float WallTiltAngle = 15f; // Grad, wie stark gekippt wird
    public const float WallTiltSpeed = 8f; // wie schnell rein/raus gelerpt wird

    #endregion
}