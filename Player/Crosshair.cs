namespace MovementRoguelike3D.Player;

public partial class Crosshair : ColorRect {
    public override void _Process(double delta) {
        Visible = !GetTree().Paused;
    }
}