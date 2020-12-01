using Godot;

public class Gun : Sprite
{
    [Signal] public delegate void Fired();

    [Export] public bool Automatic = false;
    [Export] public float ShotCooldown = .1f;

    [Export] public float ShakeDuration = .1f;
    [Export] public float ShakeFrequency = 50f;
    [Export] public float ShakeAmplitude = 0.5f;

    public Timer Timer;

    private bool shooting = false;

    public override void _Ready()
    {
        base._Ready();

        Timer = GetNode<Timer>("Timer");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event.IsActionPressed("shoot") && Timer.IsStopped() && Globals.HoveringBuilding == null && !Globals.BuildManager.Enabled)
        {
            shooting = true;
            Shoot();
        }
        else if (@event.IsActionReleased("shoot"))
        {
            shooting = false;
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (!Automatic)
        {
            return;
        }

        if (shooting && Timer.IsStopped() && Globals.DraggingBuilding == null && !Globals.BuildManager.Enabled)
        {
            Shoot();
        }
    }

    public virtual void Shoot()
    {
        GetTree().CallGroup(
            "ShakeCamera", "Shake",
            ShakeDuration,
            ShakeFrequency,
            ShakeAmplitude,
            GlobalPosition
        );

        Timer.Start(ShotCooldown);

        EmitSignal("Fired");
    }
}
