using Godot;
using System;

public class Player : KinematicBody2D
{
    // State Machine
    public enum State
    {
        Idle,
        Move,
        Turn,
    }
    public State state;

    // Movement
    [Export]
    public float acceleration = 500f;
    [Export]
    public float maxSpeed = 60f;
    [Export]
    public float friction = 800f;
    [Export]
    public float cameraForwardMult = 40f;

    public float drag;
    public Vector2 velocity = Vector2.Zero;
    public Vector2 inputVec = Vector2.Zero;

    // Rotation
    [Export]
    public float rotationSpeed = 5f;
    [Export]
    public float rotationTimerStart = 0.3f;

    public float rotationStart = 0f;
    public float rotationEnd = 0f;
    public float rotationTimer;

    public Node2D rotate;
    public Camera2D camera;
    public AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        base._Ready();

        state = State.Idle;

        rotationTimer = rotationTimerStart;
        
        rotate = GetNode<Node2D>("Rotate");
        camera = GetNode<Camera2D>("Camera");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        // Get Input
        inputVec = Vector2.Zero;
        inputVec.x += Input.GetActionStrength("right");
        inputVec.x -= Input.GetActionStrength("left");
        inputVec.y += Input.GetActionStrength("down");
        inputVec.y -= Input.GetActionStrength("up");
        inputVec = inputVec.Normalized();

        // Rotate To Mouse
        float target = GetGlobalMousePosition().AngleToPoint(rotate.GlobalPosition);

        if (target != rotationEnd)
        {
            rotationStart = rotate.GlobalRotation;
            rotationEnd = target;
            rotationTimer = rotationTimerStart;
        }

        if (rotate.GlobalRotation != rotationEnd)
        {
            rotate.GlobalRotation = Mathf.LerpAngle(rotate.GlobalRotation, rotationEnd, rotationTimer);

            rotationTimer += delta * rotationSpeed;

            rotationTimer = Math.Min(rotationTimer, 1f);
        }

        switch (state)
        {
            case State.Idle:
                animationPlayer.Play("Idle");

                if (inputVec != Vector2.Zero)
                {
                    state = State.Move;
                }

                if (Mathf.Round(rotate.GlobalRotation * 2) / 2 != Mathf.Round(rotationEnd * 2) / 2)
                {
                    state = State.Turn;
                }

                break;

            case State.Move:
                animationPlayer.Play("Move");

                // Offset Camera
                camera.Position = inputVec * cameraForwardMult;

                if (inputVec == Vector2.Zero)
                {
                    state = State.Idle;
                }

                break;

            case State.Turn:
                animationPlayer.Play("Move");

                if (Mathf.Round(rotate.GlobalRotation * 2) / 2 == Mathf.Round(rotationEnd * 2) / 2)
                {
                    state = State.Idle;
                }

                if (inputVec != Vector2.Zero)
                {
                    state = State.Move;
                }

                GD.Print(Mathf.Round(rotate.GlobalRotation * 2) / 2, Mathf.Round(rotationEnd * 2) / 2);

                break;
        }

        GD.Print(state);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        switch (state)
        {
            case State.Idle:
                break;

            case State.Move:
                if (inputVec != Vector2.Zero)
                {
                    velocity = velocity.MoveToward(inputVec * maxSpeed, acceleration * delta);
                }
                else
                {
                    velocity = velocity.MoveToward(Vector2.Zero, friction * delta);
                }

                velocity = MoveAndSlide(velocity);

                break;

            case State.Turn:
                break;
        }
    }
}
