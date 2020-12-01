using Godot;
using Godot.Collections;

public class FlyingEnemy : Entity
{
    public ProjectileEmitter ProjectileEmitter;

    private Building target;

    public override void _Ready()
    {
        base._Ready();

        ProjectileEmitter = GetNode<ProjectileEmitter>("Rotate/ProjectileEmitter");

        RecalculatePath();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (CurrentState == State.Spawning || CurrentState == State.Dead)
        {
            return;
        }

        if (IsInstanceValid(target))
        {
            Rotate(delta, target.GlobalPosition.AngleToPoint(GlobalPosition));
        }

        MoveVec = new Vector2(1, 0).Rotated(RotateGroup.GlobalRotation);

        switch (CurrentState)
        {
            case State.Idle:
                CurrentState = State.Move;
                break;

            case State.Move:
                AnimationPlayer.Play("Move");
                break;

            case State.Sprint:
                CurrentState = State.Move;
                break;
        }
    }

    public void _OnShootTimeout()
    {
        ProjectileEmitter.Emit();

        GetTree().CallGroup(
            "ShakeCamera", "Shake",
            0.15,
            45,
            0.75,
            GlobalPosition
        );
    }

    public async void RecalculatePath()
    {
        Array targetBuildings = GetTree().GetNodesInGroup("EnemyTargets");

        if (targetBuildings.Count == 0)
        {
            return;
        }

        var closest = (Building)targetBuildings[0];
        float closestDistance = GlobalPosition.DistanceTo(closest.GlobalPosition);
        foreach (Building building in targetBuildings)
        {
            if (!IsInstanceValid(building))
            {
                continue;
            }
            float dist = GlobalPosition.DistanceTo(building.GlobalPosition);

            if (dist < closestDistance && !building.Deleting)
            {
                closest = building;
                closestDistance = dist;
            }
            await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
        }

        target = closest;
    }
}
