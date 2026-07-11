namespace MovementRoguelike3D.Enemies;

public partial class Enemy : CharacterBody3D, IHealth {
    private StandardMaterial3D? material;

    public override void _Ready() {
        #region Create unique Material

        MeshInstance3D mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        Material baseMaterial = mesh.Mesh.SurfaceGetMaterial(0);
        material = (StandardMaterial3D)baseMaterial.Duplicate();
        mesh.SetSurfaceOverrideMaterial(0, material);

        #endregion
    }

    #region Health

    private float health;

    [Export]
    public float Health {
        get => health;
        set {
            health = value;
            HealthChanged(value);
        }
    }

    private void HealthChanged(float value) {
        float f = 1 - value / 100f;
        material?.SetAlbedo(new Color(1, f, f));
    }

    #endregion

    public override void _PhysicsProcess(double delta) {
        #region Death check

        if (Health <= 0) {
            QueueFree();
        }

        #endregion
    }
}