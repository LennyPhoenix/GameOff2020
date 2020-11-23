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
            Shoot();
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (!Automatic)
        {
            return;
        }

        if (Input.IsActionPressed("shoot") && Timer.IsStopped() && Globals.DraggingBuilding == null && !Globals.BuildManager.Enabled)
        {
            Shoot();
        }
    }

    public virtual void Shoot()
    {
        EmitSignal("Fired");
    }
}
