using Godot;
using System;

public class Player : KinematicBody2D
{
    // State Machine
    public enum State
    {
        Idle,
        Move,
    }
    public State CurrentState;

    // Movement
    [Export] public float Acceleration = 500f;
    [Export] public float MaxSpeed = 60f;
    [Export] public float Friction = 800f;
    [Export] public float CameraForwardMult = 40f;

    public float Drag;
    public Vector2 Velocity = Vector2.Zero;
    public Vector2 InputVec = Vector2.Zero;

    // Rotation
    [Export] public float RotationSpeed = 5f;
    [Export] public float RotationTimerStart = 0.3f;

    public float RotationStart = 0f;
    public float RotationEnd = 0f;
    public float RotationTimer;

    public Node2D RotateGroup;
    public ShakeCamera2D Camera;
    public AnimationPlayer AnimationPlayer;

    public override void _Ready()
    {
        base._Ready();

        CurrentState = State.Idle;

        RotationTimer = RotationTimerStart;
        
        RotateGroup = GetNode<Node2D>("Rotate");
        Camera = GetNode<ShakeCamera2D>("Camera");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        // Get Input
        InputVec = Vector2.Zero;
        InputVec.x += Input.GetActionStrength("right");
        InputVec.x -= Input.GetActionStrength("left");
        InputVec.y += Input.GetActionStrength("down");
        InputVec.y -= Input.GetActionStrength("up");
        InputVec = InputVec.Normalized();

        // Rotate To Mouse
        float target = GetGlobalMousePosition().AngleToPoint(RotateGroup.GlobalPosition);

        if (target != RotationEnd)
        {
            RotationStart = RotateGroup.GlobalRotation;
            RotationEnd = target;
            RotationTimer = RotationTimerStart;
        }

        if (RotateGroup.GlobalRotation != RotationEnd)
        {
            RotateGroup.GlobalRotation = Mathf.LerpAngle(RotateGroup.GlobalRotation, RotationEnd, RotationTimer);

            RotationTimer += delta * RotationSpeed;

            RotationTimer = Math.Min(RotationTimer, 1f);
        }

        switch (CurrentState)
        {
            case State.Idle:
                AnimationPlayer.Play("Idle");

                if (InputVec != Vector2.Zero)
                {
                    CurrentState = State.Move;
                }

                break;

            case State.Move:
                AnimationPlayer.Play("Move");

                // Offset Camera
                Camera.Position = InputVec * CameraForwardMult;

                if (InputVec == Vector2.Zero)
                {
                    CurrentState = State.Idle;
                }

                break;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        switch (CurrentState)
        {
            case State.Idle:
                break;

            case State.Move:
                if (InputVec != Vector2.Zero)
                {
                    Velocity = Velocity.MoveToward(InputVec * MaxSpeed, Acceleration * delta);
                }
                else
                {
                    Velocity = Velocity.MoveToward(Vector2.Zero, Friction * delta);
                }

                Velocity = MoveAndSlide(Velocity);

                break;
        }
    }
}
