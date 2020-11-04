using Godot;
using System;

public class Gun : Sprite
{
    [Export]
    public PackedScene projectile;
    [Export]
    public int accuracyAngle = 1;
    [Export]
    public int accuracySteps = 2;

    public Node2D projectiles;
    public Node2D spawnOffset;

    public override void _Ready()
    {
        base._Ready();

        Node app = GetTree().CurrentScene;
        Node2D groundEntities = app.GetNode<Node2D>("Planet").GetNode<Node2D>("GroundEntities");
        projectiles = groundEntities.GetNode<Node2D>("Projectiles");

        spawnOffset = GetNode<Node2D>("SpawnOffset");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event.IsActionPressed("shoot"))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        Projectile proj = (Projectile)projectile.Instance();

        float accuracyModifier = (float)GD.RandRange(-accuracyAngle * accuracySteps, accuracyAngle * accuracySteps) / accuracySteps;

        proj.GlobalPosition = spawnOffset.GlobalPosition;
        proj.GlobalRotation = spawnOffset.GlobalRotation + Mathf.Deg2Rad(accuracyModifier);

        projectiles.AddChild(proj);
    }
}
