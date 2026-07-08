using Godot;

namespace MovementRoguelike3D;

public static class Utils {
    public static Vector3 Lerp(Vector3 first, Vector3 second, float amount) {
        float retX = Mathf.Lerp(first.X, second.X, amount);
        float retY = Mathf.Lerp(first.Y, second.Y, amount);
        float retZ = Mathf.Lerp(first.Z, second.Z, amount);
        return new Vector3(retX, retY, retZ);
    }
}