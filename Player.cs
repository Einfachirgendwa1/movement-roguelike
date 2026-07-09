using System.Collections.Generic;
using Godot;
using MovementRoguelike3D.Components;
using MovementRoguelike3D.Coroutines;
using static MovementRoguelike3D.Prelude;

namespace MovementRoguelike3D;

public partial class Player : CharacterBody3D {
	#region Initialization

	private Camera3D? camera;
	private RayCast3D? rayCast;
	private ColorRect? crosshair;

	private const float AttackWindowDuration = 0.15f;
	private bool isAttacking = false;

	public override void _Ready() {
		Input.SetMouseMode(Input.MouseModeEnum.Captured);
		camera = GetNode<Camera3D>("Camera3D");
		rayCast = GetNode<RayCast3D>("Camera3D/RayCast3D");
		crosshair = GetNode<ColorRect>("Crosshair");

		OnFovChange += UpdateFov;
	}

	public override void _ExitTree() {
		OnFovChange -= UpdateFov;
	}

	#endregion

	public override void _PhysicsProcess(double delta) {
		AttackDetection();

		#region Collision Detection and Gravity

		bool onGround = IsOnFloor();
		bool onWall = IsOnWall();
		float wallRunningGravity = onWall ? WallRunningGravity(Velocity.Length()) : 1;

		Vector3 gravity = GetGravity() * wallRunningGravity;
		Velocity += gravity;

		#endregion

		#region Horizontal Movement

		#region Read Input

		int left = Input.IsActionPressed("Left") ? 1 : 0;
		int right = Input.IsActionPressed("Right") ? 1 : 0;
		int xAxis = right - left;

		int forward = Input.IsActionPressed("Forward") ? 1 : 0;
		int backward = Input.IsActionPressed("Backward") ? 1 : 0;
		int zAxis = backward - forward;

		bool sprinting = Input.IsActionPressed("Sprint");
		float sprintMult = sprinting ? SprintMult : 1;

		#endregion

		float airMult = !onGround ? AirMoveMultiplier : 1;
		Vector3 direction = (Transform.Basis * new Vector3(xAxis, 0, zAxis)).Normalized();
		Velocity += direction * MoveStrength * sprintMult * airMult;

		#endregion

		#region Vertical Movement

		if (Input.IsActionPressed("Jump") && onGround) {
			Velocity += new Vector3(0, JumpImpulse, 0);
		}

		if (Input.IsActionJustPressed("Jump") && onWall) {
			Vector3 wallJumpDirection = GetWallNormal().Normalized() + Vector3.Up;
			Velocity += wallJumpDirection.Normalized() * JumpImpulse;
		}

		#endregion

		Velocity *= onGround ? GroundDrag : AirDrag;
		MoveAndSlide();
	}

	#region Attack

	private void AttackDetection() {
		if (Input.IsActionJustPressed("Attack") && !isAttacking) {
			StartCoroutine(AttackCoroutine(GetMeleeDamage()));
		}
	}

	private IEnumerator<Interrupt?> AttackCoroutine(float damage) {
		isAttacking = true;

		HashSet<IHealth> alreadyHit = new();
		NextPhysicsFrame waiter = new();
		double elapsed = 0;

		while (elapsed < AttackWindowDuration) {
			if (rayCast!.GetCollider() is IHealth objectWithHp && alreadyHit.Add(objectWithHp)) {
				objectWithHp.Health -= damage;

				if (objectWithHp.IsAlive) {
					//Velocity = Velocity.Bounce(Vector3.Forward);
				} else {
					float yVelocity = Mathf.Max(-2 * Velocity.Y, JumpImpulse);
					Velocity = new Vector3(Velocity.X, yVelocity, Velocity.Z);
				}
			}

			yield return waiter;
			elapsed += waiter.Delta;
		}

		isAttacking = false;
	}

	private float GetMeleeDamage() => Velocity.Length() * 2;

	#endregion

	public override void _Process(double delta) {
		#region Escape Input

		// This should later open a pause menu
		if (Input.IsActionJustPressed("Escape")) {
			Input.SetMouseMode(Input.GetMouseMode() == Input.MouseModeEnum.Visible
				? Input.MouseModeEnum.Captured
				: Input.MouseModeEnum.Visible);
		}

		#endregion

		#region Change Crosshair Color when targeting Enemy (Debug)

		crosshair!.Color = rayCast!.IsColliding() ? Colors.Blue : Colors.White;

		#endregion

		#region Lock Camera while Wallrunning

		if (IsOnWall() && WallDot() < 0) {
			RotateY(Forward().SignedAngleTo(WallRunClampedDirection(), Vector3.Up) * 0.13f);
		}

		#endregion

		#region Camera Tilt while Wallrunning

		float targetTilt = 0f;

		if (IsOnWall() ) {
			// Vorzeichen bestimmt, ob die Wand links oder rechts ist
			float wallSide = Forward().Cross(Vector3.Up).Dot(GetWallNormal())*-1;
			targetTilt = Mathf.Sign(wallSide) * WallTiltAngle;
		}

		Vector3 camRotation = camera!.Rotation;
		camRotation.Z = Mathf.LerpAngle(camRotation.Z, Mathf.DegToRad(targetTilt), (float)delta * WallTiltSpeed);
		camera.Rotation = camRotation;

		#endregion
	}


	private void UpdateFov(float newFov) {
		camera!.Fov = newFov;
	}

	public override void _Input(InputEvent @event) {
		#region Camera Rotation with Mouse

		if (@event is InputEventMouseMotion mouseMotion && Input.GetMouseMode() == Input.MouseModeEnum.Captured) {
			bool isOnWall = IsOnWall();

			float wallNormalIntersect = WallDot();
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);

			if (isOnWall && WallDot() < wallNormalIntersect && wallNormalIntersect <= 0) {
				RotateY(mouseMotion.Relative.X * MouseSensitivity);
			}

			Vector3 rotation = camera!.Rotation;
			rotation.X -= mouseMotion.Relative.Y * MouseSensitivity;
			rotation.X = Mathf.Clamp(
				rotation.X,
				Mathf.DegToRad(MinPitch),
				Mathf.DegToRad(MaxPitch)
			);
			camera.Rotation = rotation;
		}

		#endregion
	}

	private Vector3 Forward() => camera!.GlobalBasis * Vector3.Forward;
	private float WallDot() => IsOnWall() ? Forward().Dot(GetWallNormal()) : 0;
	private Vector3 WallRunClampedDirection() => Forward().Slide(GetWallNormal());
}
