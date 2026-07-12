namespace MovementRoguelike3D.Singletons;

public abstract partial class Singleton<T> : Node where T : Singleton<T> {
    private static T? instance;

    public static T Instance => instance ?? throw new NullReferenceException("No singleton instance");

    public override void _Ready() {
        instance = (T)this;
    }
}