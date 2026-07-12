namespace MovementRoguelike3D.Menus.PauseMenu;

public partial class SaveAndQuit : Button {
    public override void _Pressed() {
        PauseMenu.Instance!.Visible = false;
        GameManager.GameState.SaveGame();
        GetTree().Quit();
    }
}