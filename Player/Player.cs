using MovementRoguelike3D.Menus.PauseMenu;

namespace MovementRoguelike3D.Player;

public partial class Player : CharacterBody3D {
    #region Initialization

    private Camera3D? camera;
    private RayCast3D? rayCast;
    private ColorRect? crosshair;
    private TextEdit? textEdit;
    private bool mouseMovementLocked;
    private float timeSpentSprinting;

    private const float AttackWindowDuration = 0.15f;
    private bool isAttacking;

    private int acceleration = 5;
    private int deceleration = 5;

    public override void _Ready() {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        camera = GetNode<Camera3D>("Camera3D");
        rayCast = GetNode<RayCast3D>("Camera3D/RayCast3D");
        crosshair = GetNode<ColorRect>("Crosshair");
        textEdit = GetNode<TextEdit>("TextEdit");

        GetTree().CurrentScene.GetNode<GameState>("GameState").OnFovChange += UpdateFov;
    }

    public override void _ExitTree() {
        State.OnFovChange -= UpdateFov;
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

        (int xAxis, int zAxis, bool sprinting, float sprintMult) = ReadInput();
        Vector3 direction = HorizontalMovement(xAxis, zAxis, onGround, sprintMult, sprinting);
        VerticalMovement(onGround, onWall);

        /*#region Drag

        float mediumDrag = onGround ? State.GroundDrag : State.AirDrag;

        if (sprinting && direction != Vector3.Zero) {
            if (onGround || onWall) {
                timeSpentSprinting += (float)delta;
            }

            float sprintDrag = SprintDrag(timeSpentSprinting, State.GroundDrag);
            Velocity = new Vector3(Velocity.X * sprintDrag, Velocity.Y * mediumDrag, Velocity.Z * sprintDrag);
        } else {
            timeSpentSprinting -= (float)delta;
            Velocity *= mediumDrag;
        }

        #endregion*/
        float horizontalSpeed = new Vector3(Velocity.X, 0, Velocity.Z).Length();
        bool isMoving = new Vector3(xAxis, 0, zAxis).Length() != 0;
        if (isMoving && horizontalSpeed < State.MaxMovementSpeed) {
            Velocity = new Vector3(
                Lerp(Velocity.X, direction.X * State.MoveStrength * sprintMult, (float)(acceleration * delta)),
                Velocity.Y,
                Lerp(Velocity.Z, direction.Z * State.MoveStrength * sprintMult, (float)(acceleration * delta)));
        } else if (isMoving && horizontalSpeed > State.MaxMovementSpeed) {
            Vector3 horizontal = new(Velocity.X, 0, Velocity.Z);
            float speed = horizontal.Length();
            Vector3 newDirection = horizontal.Normalized()
                .Lerp(direction.Normalized(), (float)(acceleration * delta))
                .Normalized();
            Vector3 newVelocity = speed * newDirection;
            newVelocity.Y = Velocity.Y;
            Velocity = newVelocity;
        } else {
            Velocity = new Vector3(
                Lerp(Velocity.X, 0.0f, (float)(deceleration * delta)),
                Velocity.Y,
                Lerp(Velocity.Z, 0.0f, (float)(deceleration * delta)));
        }


        textEdit!.Text = $"Horizontal Speed: {new Vector3(Velocity.X, 0, Velocity.Z).Length()}\n";
        textEdit.Text += $"Vertical Speed: {Velocity.Y}";
        MoveAndSlide();
    }

    // maybe Mathf.Clamp(by, 0f, 1f) instead of by
    public float Lerp(float firstFloat, float secondFloat, float by) => firstFloat * (1 - by) + secondFloat * by;

    private Vector3 HorizontalMovement(int xAxis, int zAxis, bool onGround, float sprintMult, bool sprinting) {
        Vector3 direction = (Transform.Basis * new Vector3(xAxis, 0, zAxis)).Normalized();
        // Vector3 horizontalVelocity = new(Velocity.X, 0, Velocity.Z);
        // float speed = horizontalVelocity.Length();
        // float airMult = !onGround ? State.AirMoveMultiplier : 1;
        // Velocity += direction * State.MoveStrength * sprintMult * airMult;
        //
        // if (sprinting && speed >= State.SprintAccelSpeedCap || !onGround) {
        // 	Vector3 horizontalOverride = new Vector3(Velocity.X, 0, Velocity.Z).Normalized() * speed;
        // 	Velocity = new Vector3(horizontalOverride.X, Velocity.Y, horizontalOverride.Z);
        // }
        return direction;
    }

    private static (int xAxis, int zAxis, bool sprinting, float sprintMult) ReadInput() {
        int left = Input.IsActionPressed("Left") ? 1 : 0;
        int right = Input.IsActionPressed("Right") ? 1 : 0;
        int xAxis = right - left;

        int forward = Input.IsActionPressed("Forward") ? 1 : 0;
        int backward = Input.IsActionPressed("Backward") ? 1 : 0;
        int zAxis = backward - forward;

        bool sprinting = Input.IsActionPressed("Sprint");
        float sprintMult = sprinting ? State.SprintMult : 1;
        return (xAxis, zAxis, sprinting, sprintMult);
    }

    private void VerticalMovement(bool onGround, bool onWall) {
        if (Input.IsActionPressed("Jump") && onGround) {
            Velocity += new Vector3(0, State.JumpImpulse, 0);
        } else if (Input.IsActionJustPressed("Jump") && onWall) {
            Velocity += WallJumpDirection(this).Normalized()
                        * State.JumpImpulse
                        * State.WallJumpMultiplier;

            timeSpentSprinting = 0f;
        }
    }

    public override void _Process(double delta) {
        #region Escape Input

        if (Input.IsActionJustPressed("Escape")) {
            SceneTree sceneTree = GetTree();
            Input.SetMouseMode(Input.MouseModeEnum.Visible);

            PauseMenu pauseMenu = sceneTree.CurrentScene.GetNode<PauseMenu>("PauseMenu");
            pauseMenu.Visible = true;
            pauseMenu.EscapeInputConsumed = true;

            sceneTree.Paused = true;
        }

        #endregion

        #region Change Crosshair Color when targeting Enemy (Debug)

        crosshair!.Color = rayCast!.IsColliding() ? Colors.Blue : Colors.White;

        #endregion

        #region Lock Camera while Wallrunning

        if (IsOnWallOnly() && WallDot() < 0) {
            RotateY(Forward().SignedAngleTo(WallRunClampedDirection(), Vector3.Up) * 0.13f);
        }

        #endregion

        #region Camera Tilt while Wallrunning

        float targetTilt = 0f;

        if (IsOnWall() && !IsOnFloor()) {
            // Vorzeichen bestimmt, ob die Wand links oder rechts ist
            float wallSide = Forward().Cross(Vector3.Up).Dot(GetWallNormal()) * -1;
            targetTilt = Mathf.Sign(wallSide) * State.WallTiltAngle;
        }

        Vector3 camRotation = camera!.Rotation;
        camRotation.Z = Mathf.LerpAngle(camRotation.Z, Mathf.DegToRad(targetTilt), (float)delta * State.WallTiltSpeed);
        camera.Rotation = camRotation;

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
            RotateY(-mouseMotion.Relative.X * State.MouseSensitivity);

            if (isOnWall && WallDot() < wallNormalIntersect && wallNormalIntersect <= 0) {
                RotateY(mouseMotion.Relative.X * State.MouseSensitivity);
            }

            Vector3 rotation = camera!.Rotation;
            rotation.X -= mouseMotion.Relative.Y * State.MouseSensitivity;
            rotation.X = Mathf.Clamp(
                rotation.X,
                Mathf.DegToRad(State.MinPitch),
                Mathf.DegToRad(State.MaxPitch)
            );
            camera.Rotation = rotation;
        }

        #endregion
    }

    private Vector3 Forward() => camera!.GlobalBasis * Vector3.Forward;
    private float WallDot() => IsOnWall() ? Forward().Dot(GetWallNormal()) : 0;
    private Vector3 WallRunClampedDirection() => Forward().Slide(GetWallNormal());

    #region Attack

    private void AttackDetection() {
        if (Input.IsActionJustPressed("Attack") && !isAttacking) {
            StartCoroutine(AttackCoroutine(GetMeleeDamage()));
            timeSpentSprinting = 0;
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
                    float yVelocity = Mathf.Max(-2 * Velocity.Y, State.JumpImpulse);
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
}