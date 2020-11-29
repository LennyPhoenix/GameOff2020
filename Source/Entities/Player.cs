using Godot;

public class Player : Entity
{
    // Movement
    [Export] public float CameraForwardMult = 30f;

    // Health
    [Export] public float RegenTime = 1.5f;
    [Export] public float RegenAcceleration = 0.1f;
    [Export] public float RegenAmount = 5f;
    private float currentRegenTime;

    public ShakeCamera2D Camera;
    public Timer RegenTimer;
    public HealthManager HealthManager;

    public override void _Ready()
    {
        base._Ready();
        
        Camera = GetNode<ShakeCamera2D>("Camera");
        RegenTimer = GetNode<Timer>("RegenTimer");
        HealthManager = GetNode<HealthManager>("HealthManager");

        currentRegenTime = RegenTime;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        // Get Input
        MoveVec = Vector2.Zero;
        MoveVec.x += Input.GetActionStrength("right");
        MoveVec.x -= Input.GetActionStrength("left");
        MoveVec.y += Input.GetActionStrength("down");
        MoveVec.y -= Input.GetActionStrength("up");
        MoveVec = MoveVec.Normalized();

        // Rotate To Mouse
        float target = GetGlobalMousePosition().AngleToPoint(RotateGroup.GlobalPosition);
        Rotate(delta, target);

        switch (CurrentState)
        {
            case State.Spawning:
                return;

            case State.Idle:
                AnimationPlayer.Play("Idle");

                break;

            case State.Move:
                AnimationPlayer.Play("Move");

                // Offset Camera
                Camera.Position = MoveVec * CameraForwardMult;

                break;

            case State.Sprint:
                AnimationPlayer.Play("Sprint");

                // Offset Camera
                Camera.Position = MoveVec * CameraForwardMult * SprintMult;

                break;
        }

        Vector2 rotationalVec = MoveVec;
        rotationalVec.y *= -1;
        rotationalVec = rotationalVec.Rotated(target);

        if (MoveVec == Vector2.Zero)
        {
            CurrentState = State.Idle;
        }
        else if (Input.IsActionPressed("sprint") && rotationalVec.x > .3f)
        {
            CurrentState = State.Sprint;
        }
        else
        {
            CurrentState = State.Move;
        }
    }

    public override void _OnHit(float newHealth, float maxHealth)
    {
        base._OnHit(newHealth, maxHealth);

        RegenTimer.Start(RegenTime);
        currentRegenTime = RegenTime;
    }

    public void _OnRegenTimeout()
    {
        currentRegenTime -= RegenAcceleration;
        currentRegenTime = Mathf.Max(currentRegenTime, 0.2f);
        RegenTimer.Start(currentRegenTime);

        HealthManager.Health += RegenAmount;
        HealthManager.Health = Mathf.Min(HealthManager.Health, HealthManager.MaxHealth);

        HealthBar.Health = HealthManager.Health / HealthManager.MaxHealth;

        if (Mathf.RoundToInt(HealthManager.Health) == Mathf.RoundToInt(HealthManager.MaxHealth))
        {
            return;
        }

        if (HealthBarAnimationPlayer.CurrentAnimation == "Show")
        {
            HealthBarAnimationPlayer.Seek(0.2f);
        }
        else
        {
            HealthBarAnimationPlayer.Play("Show");
        }
    }
}
