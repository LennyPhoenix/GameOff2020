using Godot;
using Godot.Collections;

public class ProjectileTurret : Turret
{
    [Export] public Array<NodePath> ProjectileEmitters = new Array<NodePath>();

    private int emitterIndex = 0;

    public override void Shoot()
    {
        var emitter = GetNode<ProjectileEmitter>(ProjectileEmitters[emitterIndex]);
        emitter.Emit();

        emitterIndex = (emitterIndex + 1) % ProjectileEmitters.Count;

        base.Shoot();
    }
}
