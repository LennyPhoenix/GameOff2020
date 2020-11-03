using Godot;
using System;

public class Projectile : Area2D
{
    [Export]
    public Vector2 velocity = new Vector2(500f, 0f);
    [Export]
    public float lifetime = 1.5f;

    public Vector2 move;

    public override void _Ready()
    {
        base._Ready();

        move = velocity.Rotated(GlobalRotation);

        Timer timer = GetNode<Timer>("Timer");
        timer.Start(lifetime);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Position += move * delta;
    }

    public void _OnBodyEntered(PhysicsBody2D body)
    {
        QueueFree();
    }

    public void _OnTimerTimeout()
    {
        QueueFree();
    }
}
