using Godot;

[Tool]
public class Gun : Sprite
{
    [Signal] public delegate void Fired();

    [Export] public PackedScene Projectile;
    [Export] public int AccuracyAngle = 1;
    [Export] public int AccuracySteps = 2;
    [Export] public int ShotCount = 1;
    [Export] public int VelocityModifier = 0;

    [Export] public bool Automatic = false;
    [Export] public float ShotCooldown = .1f;

    [Export] public float ShakeDuration = .1f;
    [Export] public float ShakeFrequency = 50f;
    [Export] public float ShakeAmplitude = 0.5f;

    public Node2D Projectiles;
    public Node2D SpawnOffset;
    public Timer Timer;

    public override void _Ready()
    {
        base._Ready();

        if (Engine.EditorHint)
        {
            return;
        }

        Projectiles = GetTree().CurrentScene.GetNode<Node2D>("Planet/Projectiles");
        SpawnOffset = GetNode<Node2D>("SpawnOffset");
        Timer = GetNode<Timer>("Timer");
    }

    public override string _GetConfigurationWarning()
    {
        if (Projectile == null)
        {
            return "Projectile property is set to null.";
        }
        return "";
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (Engine.EditorHint)
        {
            return;
        }

        if (@event.IsActionPressed("shoot") && Timer.IsStopped() && Globals.HoveringBuilding == null && !Globals.BuildPreview.Enabled)
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

        if (Input.IsActionPressed("shoot") && Timer.IsStopped() && Globals.DraggingBuilding == null && !Globals.BuildPreview.Enabled)
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        for (int i = 0; i < ShotCount; i++)
        {
            Projectile proj = (Projectile)Projectile.Instance();

            float accuracyModifier = (float)GD.RandRange(-AccuracyAngle * AccuracySteps, AccuracyAngle * AccuracySteps) / AccuracySteps;

            proj.GlobalPosition = SpawnOffset.GlobalPosition;
            proj.GlobalRotation = SpawnOffset.GlobalRotation + Mathf.Deg2Rad(accuracyModifier);

            proj.Velocity.x += (float)GD.RandRange(-VelocityModifier, VelocityModifier);

            Projectiles.AddChild(proj);
        }

        EmitSignal("Fired");
        GetTree().CallGroup(
            "ShakeCamera", "Shake",
            ShakeDuration,
            ShakeFrequency,
            ShakeAmplitude
        );

        Timer.Start(ShotCooldown);
    }
}
