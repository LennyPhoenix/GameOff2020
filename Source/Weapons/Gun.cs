using Godot;
using System;

[Tool]
public class Gun : Sprite
{
    [Signal] public delegate void Fired();

    [Export] public PackedScene Projectile;
    [Export] public int AccuracyAngle = 1;
    [Export] public int AccuracySteps = 2;

    [Export] public float ShakeDuration = .1f;
    [Export] public float ShakeFrequency = 50f;
    [Export] public float ShakeAmplitude = 0.5f;

    public Node2D Projectiles;
    public Node2D SpawnOffset;

    public override void _Ready()
    {
        base._Ready();

        if (Engine.EditorHint)
        {
            return;
        }

        Node app = GetTree().CurrentScene;
        Node2D groundEntities = app.GetNode<Node2D>("Planet").GetNode<Node2D>("GroundEntities");
        Projectiles = groundEntities.GetNode<Node2D>("Projectiles");

        SpawnOffset = GetNode<Node2D>("SpawnOffset");
    }

    public override string _GetConfigurationWarning()
    {
        if (Projectile is null)
        {
            return "Projectile property is set to null.";
        }
        return base._GetConfigurationWarning();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (Engine.EditorHint)
        {
            return;
        }

        if (@event.IsActionPressed("shoot"))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        Projectile proj = (Projectile)Projectile.Instance();

        float accuracyModifier = (float)GD.RandRange(-AccuracyAngle * AccuracySteps, AccuracyAngle * AccuracySteps) / AccuracySteps;

        proj.GlobalPosition = SpawnOffset.GlobalPosition;
        proj.GlobalRotation = SpawnOffset.GlobalRotation + Mathf.Deg2Rad(accuracyModifier);

        Projectiles.AddChild(proj);

        EmitSignal("Fired");
        GetTree().CallGroup(
            "ShakeCamera", "Shake", 
            ShakeDuration,
            ShakeFrequency,
            ShakeAmplitude
        );
    }
}