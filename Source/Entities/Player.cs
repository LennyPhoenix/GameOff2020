using Godot;
using System;

public class Player : KinematicBody2D
{
    // Movement
    [Export]
    public float acceleration = 500f;
    [Export]
    public float maxSpeed = 60f;
    [Export]
    public float friction = 800f;
    [Export]
    public float cameraForwardMult = 30f;

    public float drag;
    public Vector2 velocity = Vector2.Zero;
    public Vector2 inputVec = Vector2.Zero;

    // Rotation
    [Export]
    public float rotationSpeed = 5f;
    [Export]
    public float rotationTimerStart = 0.1f;

    public float rotationStart = 0f;
    public float rotationEnd = 0f;
    public float rotationTimer;

    public Node2D rotate;
    public Camera2D camera;

    public override void _Ready()
    {
        base._Ready();

        rotationTimer = rotationTimerStart;
        
        rotate = GetNode<Node2D>("Rotate");
        camera = GetNode<Camera2D>("Camera");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        inputVec = Vector2.Zero;
        inputVec.x += Input.GetActionStrength("right");
        inputVec.x -= Input.GetActionStrength("left");
        inputVec.y += Input.GetActionStrength("down");
        inputVec.y -= Input.GetActionStrength("up");
        inputVec = inputVec.Normalized();

        camera.Position = inputVec * cameraForwardMult;

        // Get target rotation
        float target = GetGlobalMousePosition().AngleToPoint(rotate.GlobalPosition);

        // Reset lerp if target is different.
        if (target != rotationEnd)
        {
            rotationStart = rotate.Rotation;
            rotationEnd = target;
            rotationTimer = rotationTimerStart;
        }

        // Reset lerp if angle is reached, else lerp.
        if (rotate.Rotation != rotationEnd)
        {
            rotate.Rotation = Mathf.LerpAngle(rotate.Rotation, rotationEnd, rotationTimer);

            rotationTimer += delta * rotationSpeed;

            rotationTimer = Math.Min(rotationTimer, 1f);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (inputVec != Vector2.Zero)
        {
            velocity = velocity.MoveToward(inputVec * maxSpeed, acceleration * delta);
        }
        else
        {
            velocity = velocity.MoveToward(Vector2.Zero, friction * delta);
        }

        velocity = MoveAndSlide(velocity);
    }
}
