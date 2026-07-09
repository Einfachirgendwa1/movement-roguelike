namespace MovementRoguelike3D.Coroutines;

public class WaitForFrame : Interrupt {
    public override bool Frame(double delta) => true;
}