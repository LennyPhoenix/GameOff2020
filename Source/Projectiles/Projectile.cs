using Godot;

public class Projectile : Area2D
{
    [Export] public Vector2 Velocity = new Vector2(500f, 0f);
    [Export] public float RotationSpeed = 0f;
    [Export] public float Lifetime = 1.5f;
    [Export] public float Damage = 10f;

    public Sprite Sprite;

    public Vector2 Move;

    public override void _Ready()
    {
        base._Ready();

        Sprite = GetNode<Sprite>("Sprite");

        Move = Velocity.Rotated(GlobalRotation);

        Timer timer = GetNode<Timer>("Timer");
        timer.Start(Lifetime);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        Sprite.RotationDegrees += RotationSpeed * delta;
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
