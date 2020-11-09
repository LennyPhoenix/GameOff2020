using Godot;

public class Projectile : Area2D
{
    [Export] public Vector2 Velocity = new Vector2(500f, 0f);
    [Export] public float Lifetime = 1.5f;

    public Vector2 Move;

    public override void _Ready()
    {
        base._Ready();

        Move = Velocity.Rotated(GlobalRotation);

        Timer timer = GetNode<Timer>("Timer");
        timer.Start(Lifetime);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Position += Move * delta;
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
