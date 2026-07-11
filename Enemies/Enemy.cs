namespace MovementRoguelike3D.Enemies;

public partial class Enemy : CharacterBody3D, IHealth
{
	private StandardMaterial3D? material;

	private NavigationAgent3D _navigationAgent = null!;
	private Node3D? _player;
	private float _movementSpeed = 2.0f;

	public Vector3 MovementTarget
	{
		get => _navigationAgent.TargetPosition;
		set => _navigationAgent.TargetPosition = value;
	}

	public override void _Ready()
	{
		base._Ready();

		#region Create unique Material

		var mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		var baseMaterial = mesh.Mesh.SurfaceGetMaterial(0);
		material = (StandardMaterial3D)baseMaterial.Duplicate();
		mesh.SetSurfaceOverrideMaterial(0, material);

		#endregion

		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

		// These values need to be adjusted for the actor's speed
		// and the navigation layout.
		_navigationAgent.PathDesiredDistance = 0.5f;
		_navigationAgent.TargetDesiredDistance = 0.5f;

		// Make sure to not await during _Ready.
		Callable.From(ActorSetup).CallDeferred();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		#region Death check

		if (Health <= 0) QueueFree();

		#endregion

		if (_player != null)
			MovementTarget = _player.GlobalPosition; // keep chasing, updated every frame

		var currentAgentPosition = GlobalTransform.Origin;
		var nextPathPosition = _navigationAgent.GetNextPathPosition();

		Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * _movementSpeed;
		MoveAndSlide();
	}

	private void ActorSetup()
	{
		var players = GetTree().GetNodesInGroup("player");
		GD.Print($"Found {players.Count} players in group");
		if (players.Count > 0 && players[0] is Node3D playerNode)
		{
			_player = playerNode;
			MovementTarget = _player.GlobalPosition;
		}
	}

	#region Health

	private float health;

	[Export]
	public float Health
	{
		get => health;
		set
		{
			health = value;
			HealthChanged(value);
		}
	}

	private void HealthChanged(float value)
	{
		var f = 1 - value / 100f;
		material?.SetAlbedo(new Color(1, f, f));
	}

	#endregion
}
