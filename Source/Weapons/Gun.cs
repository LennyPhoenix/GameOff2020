using Godot;
using System;

public class Gun : Sprite
{
    [Export]
    public PackedScene projectile;
    [Export]
    public Vector2 spawnOffset = Vector2.Zero;

    public Node2D projectiles;

    public override void _Ready()
    {
        base._Ready();

        Node app = GetTree().CurrentScene;
        Node2D groundEntities = app.GetNode<Node2D>("Planet").GetNode<Node2D>("GroundEntities");
        projectiles = groundEntities.GetNode<Node2D>("Projectiles");
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

        proj.GlobalPosition = GlobalPosition + spawnOffset.Rotated(GlobalRotation);
        proj.GlobalRotation = GlobalRotation;

        projectiles.AddChild(proj);
    }
}
