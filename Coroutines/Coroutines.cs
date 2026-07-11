namespace MovementRoguelike3D.Coroutines;

public partial class Coroutines : Node {
    private static Coroutines? instance;
    private readonly List<IEnumerator<Interrupt?>> coroutines = [];

    public Coroutines() {
        instance = this;
    }

    public static void StartCoroutine(IEnumerator<Interrupt?> coroutine) {
        coroutine.MoveNext();
        instance!.coroutines.Add(coroutine);
    }

    public override void _PhysicsProcess(double delta) {
        coroutines.RemoveAll(coroutine => {
            if (coroutine.Current?.Physic(delta) ?? true) {
                return !coroutine.MoveNext();
            }

            return false;
        });
    }

    public override void _Process(double delta) {
        coroutines.RemoveAll(coroutine => {
            if (coroutine.Current?.Frame(delta) ?? false) {
                return !coroutine.MoveNext();
            }

            return false;
        });
    }
}