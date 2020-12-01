using Godot;

public class SlimeWithGun : Enemy
{
    public ProjectileEmitter BottomEmitter;

    public override void _Ready()
    {
        BottomEmitter = GetNode<ProjectileEmitter>("Rotate/BottomEmitter");

        base._Ready();
    }

    public override void _OnTimeout()
    {
        ProjectileEmitter.Emit();
        BottomEmitter.Emit();
        GetTree().CallGroup(
            "ShakeCamera", "Shake",
            0.15,
            45,
            0.75,
            GlobalPosition
        );
    }
}
