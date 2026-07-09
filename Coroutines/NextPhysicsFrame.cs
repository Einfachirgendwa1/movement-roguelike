namespace MovementRoguelike3D.Coroutines;

public sealed class NextPhysicsFrame : Interrupt {
    public double Delta { get; private set; }

    public override bool Physic(double delta) {
        Delta = delta;
        return true;
    }
}