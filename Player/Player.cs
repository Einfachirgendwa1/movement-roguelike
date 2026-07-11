namespace MovementRoguelike3D.Player;

public partial class Player : CharacterBody3D {
    #region Initialization

    private Camera3D? camera;
    private RayCast3D? rayCast;
    private ColorRect? crosshair;
    private bool mouseMovementLocked;

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

    private void AttackDetection() {
        if (Input.IsActionJustPressed("Attack") && rayCast!.GetCollider() is IHealth objectWithHp) {
            objectWithHp.Health -= GetMeleeDamage();

            if (objectWithHp.IsAlive) {
                Velocity = Velocity.Bounce(Vector3.Forward);
            } else {
                float yVelocity = Mathf.Max(-2 * Velocity.Y, JumpImpulse);
                Velocity = new Vector3(Velocity.X, yVelocity, Velocity.Z);
            }
        }
    }

    private float GetMeleeDamage() => Velocity.Length() * 2;

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
            StartCoroutine(WallRunCam());
        }

        #endregion
    }


    private void UpdateFov(float newFov) {
        camera!.Fov = newFov;
    }

    public override void _Input(InputEvent @event) {
        #region Camera Rotation with Mouse

        bool inGame = Input.GetMouseMode() == Input.MouseModeEnum.Captured;
        if (@event is InputEventMouseMotion mouseMotion && inGame && !mouseMovementLocked) {
            bool isOnWall = IsOnWall();

            float wallNormalIntersect = WallDot();
            GD.Print("Pre movement ", WallDot());
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            if (isOnWall && WallDot() < wallNormalIntersect && WallDot() <= 0) {
                RotateY(mouseMotion.Relative.X * MouseSensitivity);
                GD.Print("Reset back to ", WallDot());
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

    private IEnumerator<Interrupt?> WallRunCam() {
        if (WallDot() >= 0) yield break;
        mouseMovementLocked = true;
        GD.Print("Locked");

        Vector3 target = WallRunClampedDirection();
        for (int i = 0; i <= 10; i++) {
            RotateY(Forward().SignedAngleTo(target, Vector3.Up) * 0.13f);
            yield return new WaitForFrame();
        }

        RotateY(Forward().SignedAngleTo(target, Vector3.Up) * 1.01f);

        mouseMovementLocked = false;
        GD.Print($"Unlocked at {WallDot()}");
    }

    private Vector3 Forward() => camera!.GlobalBasis * Vector3.Forward;
    private float WallDot() => IsOnWall() ? Forward().Dot(GetWallNormal()) : 0;
    private Vector3 WallRunClampedDirection() => Forward().Slide(GetWallNormal());
}