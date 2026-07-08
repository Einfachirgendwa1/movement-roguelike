using Godot;

namespace MovementRoguelike3D;

public partial class Player : CharacterBody3D {
    public override void _Ready() {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
    }

    public override void _PhysicsProcess(double delta) {
        Velocity += GetGravity();

        int left = Input.IsActionPressed("Left") ? 1 : 0;
        int right = Input.IsActionPressed("Right") ? 1 : 0;
        int xAxis = right - left;

        int forward = Input.IsActionPressed("Forward") ? 1 : 0;
        int backward = Input.IsActionPressed("Backward") ? 1 : 0;
        int zAxis = backward - forward;

        Vector3 direction = new(xAxis, 0, zAxis);
        Vector3 movement = direction.Normalized() * Settings.MovementSpeed;
        movement.Y = Velocity.Y;
        Velocity = movement;

        MoveAndSlide();
    }

    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("Escape")) {
            Input.SetMouseMode(Input.MouseModeEnum.Visible);
        }
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseMotion mouseMotion) { }
    }
}