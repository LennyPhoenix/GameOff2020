using Godot;
using System;

public class Entity : KinematicBody2D
{
    // State Machine
    public enum State
    {
        Spawning,
        Idle,
        Move,
        Sprint,
    }
    public State CurrentState;

    // Movement
    [Export] public float Acceleration = 500f;
    [Export] public float MaxSpeed = 60f;
    [Export] public float Friction = 400f;
    [Export] public float SprintMult = 3f;

    public float Drag;
    public Vector2 Velocity = Vector2.Zero;
    public Vector2 MoveVec = Vector2.Zero;

    // Rotation
    [Export] public float RotationSpeed = 5f;
    [Export] public float RotationTimerStart = 0.3f;

    public float RotationStart = 0f;
    public float RotationEnd = 0f;
    public float RotationTimer;

    public Node2D RotateGroup;
    public AnimationPlayer AnimationPlayer;

    public Control UI;
    public HealthBar HealthBar;
    public AnimationPlayer HealthBarAnimationPlayer;

    public Node2D EntityUI;

    public override void _Ready()
    {
        base._Ready();

        CurrentState = State.Spawning;

        RotationTimer = RotationTimerStart;

        RotateGroup = GetNode<Node2D>("Rotate");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        UI = GetNode<Control>("UI");
        HealthBar = GetNode<HealthBar>("UI/HealthBar");
        HealthBarAnimationPlayer = GetNode<AnimationPlayer>("UI/HealthBar/AnimationPlayer");

        EntityUI = GetTree().CurrentScene.GetNode<Node2D>("Planet/EntityUI");

        RemoveChild(UI);
        EntityUI.AddChild(UI);
        UI.RectGlobalPosition = GlobalPosition;
        UI.Name = Name;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        switch (CurrentState)
        {
            case State.Spawning:
                return;

            case State.Idle:
                Velocity = Velocity.MoveToward(Vector2.Zero, Friction * delta);
                break;

            case State.Move:
                Velocity = Velocity.MoveToward(MoveVec * MaxSpeed, Acceleration * delta);
                break;

            case State.Sprint:
                Velocity = Velocity.MoveToward(MoveVec * MaxSpeed * SprintMult, Acceleration * SprintMult * delta);
                break;
        }

        Velocity = MoveAndSlide(Velocity);

        UI.RectGlobalPosition = GlobalPosition;
    }

    public void _OnAnimationFinished(string animName)
    {
        if (animName == "Spawn")
        {
            CurrentState = State.Idle;
        }
    }

    public virtual void _OnHit(float newHealth, float maxHealth)
    {
        HealthBar.Health = newHealth / maxHealth;

        if (HealthBarAnimationPlayer.CurrentAnimation == "Show")
        {
            HealthBarAnimationPlayer.Seek(0.2f);
        }
        else
        {
            HealthBarAnimationPlayer.Play("Show");
        }

        if (newHealth <= 0)
        {
            Kill();
        }
    }

    public virtual void Kill()
    {
        UI.QueueFree();
        QueueFree();
    }

    public void Rotate(float delta, float target)
    {
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

            RotationTimer = Mathf.Min(RotationTimer, 1f);
        }
    }
}
