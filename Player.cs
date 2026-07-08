using Godot;
using static MovementRoguelike3D.Settings;

namespace MovementRoguelike3D;

public partial class Player : CharacterBody3D {
    private Node3D? cameraPivot;

    public override void _Ready() {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        cameraPivot = GetNode<Node3D>("Camera3D");
    }

    public override void _PhysicsProcess(double delta) {
        #region Movement

        bool onGround = IsOnFloor();
        Velocity += GetGravity();

        #region Horizontal Movement

        #region Read Input

        int left = Input.IsActionPressed("Left") ? 1 : 0;
        int right = Input.IsActionPressed("Right") ? 1 : 0;
        int xAxis = right - left;

        int forward = Input.IsActionPressed("Forward") ? 1 : 0;
        int backward = Input.IsActionPressed("Backward") ? 1 : 0;
        int zAxis = backward - forward;

        bool sprinting = Input.IsActionPressed("Sprint");
        float sprintMult = sprinting ? SprintMult : 1;

        #endregion

        float airMult = !onGround ? AirMoveMultiplier : 1;
        Vector3 direction = (Transform.Basis * new Vector3(xAxis, 0, zAxis)).Normalized();
        Velocity += direction * MoveStrength * sprintMult * airMult;
        GD.Print(Velocity.Length());

        #endregion

        #region Vertical Movement

        if (Input.IsActionPressed("Jump") && onGround) {
            Velocity += new Vector3(0, JumpImpulse, 0);
        }

        #endregion

        Velocity *= onGround ? GroundDrag : AirDrag;
        MoveAndSlide();

        #endregion
    }

    public override void _Process(double delta) {
        #region Escape Input

        // This should later open a pause menu
        if (Input.IsActionJustPressed("Escape")) {
            Input.SetMouseMode(Input.GetMouseMode() == Input.MouseModeEnum.Visible
                ? Input.MouseModeEnum.Captured
                : Input.MouseModeEnum.Visible);
        }

        #endregion
    }

    public override void _Input(InputEvent @event) {
        #region Camera Rotation with Mouse

        if (@event is InputEventMouseMotion mouseMotion) {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            if (cameraPivot != null) {
                Vector3 rotation = cameraPivot!.Rotation;
                rotation.X -= mouseMotion.Relative.Y * MouseSensitivity;
                rotation.X = Mathf.Clamp(
                    rotation.X,
                    Mathf.DegToRad(MinPitch),
                    Mathf.DegToRad(MaxPitch)
                );
                cameraPivot.Rotation = rotation;
            }
        }

        #endregion
    }
}