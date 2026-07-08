#nullable enable
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
        Velocity += GetGravity();

        int left = Input.IsActionPressed("Left") ? 1 : 0;
        int right = Input.IsActionPressed("Right") ? 1 : 0;
        int xAxis = right - left;

        int forward = Input.IsActionPressed("Forward") ? 1 : 0;
        int backward = Input.IsActionPressed("Backward") ? 1 : 0;
        int zAxis = backward - forward;

        int jump = Input.IsActionPressed("Jump") ? 1 : 0;
        int yAxis = jump;
        
        
        Vector3 direction = Transform.Basis * new Vector3(xAxis, 0, zAxis);
        Vector3 movement = direction.Normalized() * MovementSpeed;
        movement.Y = Velocity.Y+yAxis * (IsOnFloor() ? JumpImpulse : 0);
        Velocity = movement;
        MoveAndSlide();
    }

    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("Escape")) {
            Input.SetMouseMode(Input.MouseModeEnum.Visible);
        }
    }

    public override void _Input(InputEvent @event) {
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
    }
}