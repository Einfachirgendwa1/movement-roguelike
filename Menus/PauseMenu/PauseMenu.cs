namespace MovementRoguelike3D.Menus.PauseMenu;

public partial class PauseMenu : Control {
    public static PauseMenu? Instance;
    public bool EscapeInputConsumed;

    public override void _Ready() {
        Instance = this;
    }

    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("Escape") && !EscapeInputConsumed) {
            Resume();
        }

        EscapeInputConsumed = false;
    }

    public void Resume() {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        GetTree().Paused = false;
        Visible = false;
    }
}