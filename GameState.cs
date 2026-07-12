namespace MovementRoguelike3D;

[GlobalClass]
public partial class GameState : Node {
    private RandomNumberGenerator? rng;

    public void SaveGame() {
        PackedScene packedScene = new();

        if (packedScene.Pack(GetTree().CurrentScene) != Error.Ok) {
            GD.PushError("Couldn't pack scene tree!");
            return;
        }

        if (ResourceSaver.Save(packedScene, GameManager.SaveFileName) != Error.Ok) {
            GD.PushError("Couldn't save scene data!");
        }
    }

    public override void _Ready() {
        rng ??= GameManager.Instance.RandomNumberGenerator;
    }
}