using Godot;
using Godot.Collections;

public class Enemy : Entity
{
    public Planet Planet;
    public Timer ShootTimer;
    public Area2D TargetArea;
    public Area2D PriorityTargetArea;
    public ProjectileEmitter ProjectileEmitter;

    private Building lastTarget;
    private Array<Vector2> path = new Array<Vector2>();
    private Thread pathThread = new Thread();

    public override void _Ready()
    {
        base._Ready();

        Planet = GetTree().CurrentScene.GetNode<Planet>("Planet");
        ShootTimer = GetNode<Timer>("ShootTimer");
        TargetArea = GetNode<Area2D>("TargetArea");
        PriorityTargetArea = GetNode<Area2D>("PriorityTargetArea");

        ProjectileEmitter = GetNode<ProjectileEmitter>("Rotate/ProjectileEmitter");

        RecalculatePath();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (CurrentState == State.Spawning)
        {
            return;
        }

        if (path.Count == 0)
        {
            CurrentState = State.Idle;
            ShootTimer.Stop();
            return;
        }

        if (GlobalPosition.DistanceTo(path[0]) < 12)
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
        else if (PriorityTargetArea.GetOverlappingBodies().Contains(lastTarget))
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
                rotateTarget = lastTarget.GlobalPosition.AngleToPoint(RotateGroup.GlobalPosition);
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

    public override void _ExitTree()
    {
        base._ExitTree();

        if (pathThread.IsActive())
        {
            pathThread.WaitToFinish();
        }
    }

    public void _OnTimeout()
    {
        ProjectileEmitter.Emit();
    }

    public void RecalculatePath()
    {
        Array targetBuildings = GetTree().GetNodesInGroup("EnemyTargets");

        var closest = (Building)targetBuildings[0];
        float closestDistance = GlobalPosition.DistanceTo(closest.GlobalPosition);
        foreach (Building building in targetBuildings)
        {
            float dist = GlobalPosition.DistanceTo(building.GlobalPosition);

            if (dist < closestDistance && !building.Deleting)
            {
                closest = building;
                closestDistance = dist;
            }
        }

        if (lastTarget != closest)
        {
            lastTarget = closest;

            path = new Array<Vector2>();
            if (pathThread.IsActive())
            {
                pathThread.WaitToFinish();
            }
            pathThread.Start(this, "PathThread");
        }
    }

    public void PathThread(object userData)
    {
        Vector2[] newPath = Planet.GetSimplePath(GlobalPosition, lastTarget.GlobalPosition);

        foreach (Vector2 point in newPath)
        {
            path.Add(point);
        }
    }
}
