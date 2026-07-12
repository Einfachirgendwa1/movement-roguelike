namespace MovementRoguelike3D.Menus.MainMenu;

public partial class Quit : Button {
    public override void _Pressed() {
        GetTree().Quit();
    }
}