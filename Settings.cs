using System;
using Godot;

namespace MovementRoguelike3D;

public partial class Settings : Node2D {
    #region Abilities

    public static float MoveStrength => 1.5f;
    public static float AirMoveMultiplier => 0.3f;
    public static float JumpImpulse => 20f;
    public static float SprintMult => 1.5f;

    #endregion

    #region Input

    public static float MouseSensitivity => .003f;
    public static float MinPitch => -90;
    public static float MaxPitch => 90;

    #endregion

    #region Drag

    public static float GroundDrag => 0.80f;
    public static float AirDrag => 0.95f;

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

    #endregion
}