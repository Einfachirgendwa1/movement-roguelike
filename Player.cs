using Godot;

namespace MovementRoguelike3D;

public partial class Player : CharacterBody3D {
    public override void _PhysicsProcess(double delta) {
        Velocity += GetGravity();

        MoveAndSlide();
    }
}