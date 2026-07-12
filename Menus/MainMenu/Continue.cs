namespace MovementRoguelike3D.Menus.MainMenu;

public partial class Continue : Button {
    public override void _Ready() {
        Visible = GameManager.SaveGameExists();
    }

    public override void _Pressed() {
        GameManager.LoadSave();
    }
}