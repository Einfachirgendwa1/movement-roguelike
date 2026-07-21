namespace MovementRoguelike3D;

[GlobalClass]
public partial class GameState : Node {
    private RandomNumberGenerator? rng;
    private static GameState? instance;

    public static GameState State => instance ?? throw new NullReferenceException("GameState not initialized!");

    #region Stats

    #region Abilities

    [Export] public float MoveStrength = 5f;
    [Export] public float AirMoveMultiplier = 0.3f;
    [Export] public float JumpImpulse = 15f;
    [Export] public float SprintMult = 2f;
    [Export] public float WallJumpMultiplier = 1.5f;
    [Export] public float SprintAccelSpeedCap = 9f;
    [Export] public float MaxMovementSpeed = 10f;

    public static Func<float, float> WallRunningGravity => runSpeed => Mathf.Min(1f, 1f / runSpeed);
    public static Vector3 WallJumpDirection(Player.Player player) => player.GetWallNormal().Normalized() + Vector3.Up;

    #endregion

    #region Input

    [Export] public float MouseSensitivity = .003f;
    [Export] public float MinPitch = -90;
    [Export] public float MaxPitch = 90;

    #endregion

    #region Drag

    [Export] public float GroundDrag = 0.80f;
    [Export] public float AirDrag = 0.95f;

    public static float SprintDrag(float x, float drag) =>
        Mathf.Lerp(1, drag, Mathf.Clamp((x - 2) * 0.05f, 0f, 1f));

    #endregion

    #region Visual

    private float fov = 90;
    public event Action<float>? OnFovChange;

    [Export]
    public float Fov {
        get => fov;
        set {
            fov = value;
            OnFovChange?.Invoke(fov);
        }
    }

    /// <summary>
    /// Grad, wie stark gekippt wird
    /// </summary>
    [Export] public float WallTiltAngle = 15f;

    /// <summary>
    /// wie schnell rein/raus gelerpt wird
    /// </summary>
    [Export] public float WallTiltSpeed = 8f;

    #endregion

    #endregion

    public void SaveGame() {
        PackedScene packedScene = new();

        if (packedScene.Pack(GetTree().CurrentScene) != Error.Ok) {
            GD.PushError("Couldn't pack scene tree!");
            return;
        }

        if (ResourceSaver.Save(packedScene, GameManager.SaveFileName) != Error.Ok) {
            GD.PushError("Couldn't save scene data!");
        }
    }

    public override void _Ready() {
        instance = this;
        rng ??= GameManager.Instance.RandomNumberGenerator;
    }
}