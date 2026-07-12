namespace MovementRoguelike3D.Menus.PauseMenu;

public partial class Resume : Button {
    public override void _Pressed() {
        PauseMenu.Instance?.Resume();
    }
}