namespace MovementRoguelike3D.Singletons;

public partial class GameManager : Singleton<GameManager> {
    public static string SaveFileName => "res://test_scene_data.tscn";
    public RandomNumberGenerator RandomNumberGenerator = new();
    public static GameState GameState => Instance.GetTree().CurrentScene.GetNode<GameState>("GameState");

    public static void NewGame(ulong? seed = null) {
        if (seed is { } s) Instance.RandomNumberGenerator.Seed = s;
        LoadScene(ResourceLoader.Load<PackedScene>("res://Levels/Level1.tscn"));
    }

    public static void LoadSave() {
        LoadScene(ResourceLoader.Load<PackedScene>(SaveFileName));
    }

    private static void LoadScene(PackedScene packedScene) {
        if (Instance.GetTree().ChangeSceneToPacked(packedScene) != Error.Ok) {
            GD.PushError("Couldn't load save data!");
        }
    }

    public static bool SaveGameExists() => ResourceLoader.Exists(SaveFileName);
}