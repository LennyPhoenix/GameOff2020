using Godot;

[Tool]
public class ProjectileEmitter : Node2D
{
    [Export] public PackedScene Projectile;
    [Export] public int AccuracyAngle = 1;
    [Export] public int AccuracySteps = 2;
    [Export] public int ShotCount = 1;
    [Export] public int VelocityModifier = 0;
    [Export] public int RotationModifier = 0;

    public Node2D Projectiles;

    public override void _Ready()
    {
        base._Ready();

        if (Engine.EditorHint)
        {
            return;
        }

        Projectiles = GetTree().CurrentScene.GetNode<Node2D>("Planet/Projectiles");
    }

    public override string _GetConfigurationWarning()
    {
        if (Projectile == null)
        {
            return "Projectile property is set to null.";
        }
        return "";
    }

    public void Emit()
    {
        for (int i = 0; i < ShotCount; i++)
        {
            Projectile proj = (Projectile)Projectile.Instance();

            float accuracyModifier = (float)GD.RandRange(-AccuracyAngle * AccuracySteps, AccuracyAngle * AccuracySteps) / AccuracySteps;

            proj.GlobalPosition = GlobalPosition;
            proj.GlobalRotation = GlobalRotation + Mathf.Deg2Rad(accuracyModifier);

            proj.Velocity.x += (float)GD.RandRange(-VelocityModifier, VelocityModifier);
            proj.RotationSpeed += (float)GD.RandRange(-RotationModifier, RotationModifier);

            Projectiles.AddChild(proj);
        }
    }
}
