namespace MovementRoguelike3D.Coroutines;

public abstract class Interrupt {
    public virtual bool Physic(double delta) => false;
    public virtual bool Frame(double delta) => false;
}