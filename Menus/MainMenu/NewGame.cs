namespace MovementRoguelike3D.Menus.MainMenu;

public partial class NewGame : Button {
    public override void _Pressed() {
        GameManager.NewGame();
    }
}