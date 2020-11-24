using Godot;

public class ProjectileGun : Gun
{
    public ProjectileEmitter ProjectileEmitter;

    public override void _Ready()
    {
        base._Ready();

        ProjectileEmitter = GetNode<ProjectileEmitter>("ProjectileEmitter");
    }

    public override void Shoot()
    {
        ProjectileEmitter.Emit();

        base.Shoot();
    }
}
