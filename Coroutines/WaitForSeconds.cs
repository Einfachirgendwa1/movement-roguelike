namespace MovementRoguelike3D.Coroutines;

public class WaitForSeconds(double n) : Interrupt {
    private double n = n;

    public override bool Physic(double delta) {
        n -= delta;
        return n <= 0;
    }
}