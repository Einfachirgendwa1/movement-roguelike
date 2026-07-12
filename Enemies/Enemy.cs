using Godot.Collections;

namespace MovementRoguelike3D.Enemies;

public partial class Enemy : CharacterBody3D, IHealth {
    private StandardMaterial3D? material;

    private NavigationAgent3D? navigationAgent;
    private Node3D? player;
    private float movementSpeed = 2.0f;

    private Vector3 MovementTarget {
        get => navigationAgent!.TargetPosition;
        set => navigationAgent!.TargetPosition = value;
    }

    public override void _Ready() {
        base._Ready();

        #region Create unique Material

        MeshInstance3D? mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        Material? baseMaterial = mesh.Mesh.SurfaceGetMaterial(0);
        material = (StandardMaterial3D)baseMaterial.Duplicate();
        mesh.SetSurfaceOverrideMaterial(0, material);

        #endregion

        navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

        // These values need to be adjusted for the actor's speed
        // and the navigation layout.
        navigationAgent.PathDesiredDistance = 0.5f;
        navigationAgent.TargetDesiredDistance = 0.5f;

        // Make sure to not await during _Ready.
        Callable.From(ActorSetup).CallDeferred();
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);

        #region Death check

        if (Health <= 0) QueueFree();

        #endregion

        if (player != null)
            MovementTarget = player.GlobalPosition; // keep chasing, updated every frame

        Vector3 currentAgentPosition = GlobalTransform.Origin;
        Vector3 nextPathPosition = navigationAgent!.GetNextPathPosition();

        Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * movementSpeed;
        MoveAndSlide();
    }

    private void ActorSetup() {
        Array<Node>? players = GetTree().GetNodesInGroup("player");
        if (players.Count > 0 && players[0] is Node3D playerNode) {
            player = playerNode;
            MovementTarget = player.GlobalPosition;
        }
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
}