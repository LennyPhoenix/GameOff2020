using Godot;
using Godot.Collections;

public class Enemy : Entity
{
    [Export] public float NextPathPoint = 12f;

    public NavigationManager NavigationManager;

    public Timer ShootTimer;
    public Area2D TargetArea;
    public Area2D PriorityTargetArea;
    public ProjectileEmitter ProjectileEmitter;

    private Building target;
    private Array<Vector2> path = new Array<Vector2>();

    public override void _Ready()
    {
        base._Ready();

        NavigationManager = GetTree().CurrentScene.GetNode<NavigationManager>("Planet/NavigationManager");

        ShootTimer = GetNode<Timer>("ShootTimer");
        TargetArea = GetNode<Area2D>("TargetArea");
        PriorityTargetArea = GetNode<Area2D>("PriorityTargetArea");
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

        if (path.Count == 0)
        {
            CurrentState = State.Idle;
            ShootTimer.Stop();
            RecalculatePath();
            return;
        }

        if (GlobalPosition.DistanceTo(path[0]) < NextPathPoint || (IsInstanceValid(target) && GlobalPosition.DistanceTo(path[0]) < 32 && path[0].DistanceTo(target.GlobalPosition) > GlobalPosition.DistanceTo(target.GlobalPosition)))
        {
            path.RemoveAt(0);

            if (path.Count == 0)
            {
                return;
            }
        }

        MoveVec = GlobalPosition.DirectionTo(path[0]);

        Array overlappingBodies = TargetArea.GetOverlappingBodies();
        if (overlappingBodies.Count == 0)
        {
            CurrentState = State.Sprint;
            ShootTimer.Stop();
        }
        else if (PriorityTargetArea.GetOverlappingBodies().Contains(target))
        {
            CurrentState = State.Idle;
            if (ShootTimer.IsStopped())
            {
                ShootTimer.Start();
            }
        }
        else
        {
            CurrentState = State.Move;
            if (ShootTimer.IsStopped())
            {
                ShootTimer.Start();
            }
        }

        float rotateTarget;
        switch (CurrentState)
        {
            case State.Spawning:
                return;

            case State.Idle:
                AnimationPlayer.Play("Idle");
                rotateTarget = target.GlobalPosition.AngleToPoint(RotateGroup.GlobalPosition);
                Rotate(delta, rotateTarget);
                break;

            case State.Move:
                AnimationPlayer.Play("Move");
                var closest = (PhysicsBody2D)overlappingBodies[0];
                float closestDistance = GlobalPosition.DistanceTo(closest.GlobalPosition);
                foreach (PhysicsBody2D body in overlappingBodies)
                {
                    float dist = GlobalPosition.DistanceTo(body.GlobalPosition);
                    if (dist < closestDistance)
                    {
                        closest = body;
                        closestDistance = dist;
                    }
                }
                rotateTarget = closest.GlobalPosition.AngleToPoint(RotateGroup.GlobalPosition);
                Rotate(delta, rotateTarget);

                break;

            case State.Sprint:
                AnimationPlayer.Play("Sprint");
                rotateTarget = MoveVec.Angle();
                Rotate(delta, rotateTarget);
                break;
        }
    }

    public virtual void _OnTimeout()
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

        if (!IsInstanceValid(closest))
        {
            CallDeferred("RecalculatePath");
            return;
        }

        target = closest;

        NavigationManager.QueueAgent(this, target.GlobalPosition, (Array<Vector2> newPath) =>
        {
            path = newPath;
        });
    }

    public override void Kill()
    {
        base.Kill();

        NavigationManager.RemoveAgent(this);
    }
}
