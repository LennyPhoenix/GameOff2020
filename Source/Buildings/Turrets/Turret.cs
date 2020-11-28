using Godot;
using Godot.Collections;

public class Turret : Building
{
    [Export] public Dictionary<Item, int> Uses = new Dictionary<Item, int>();

    // Rotation
    [Export] public float RotationSpeed = 5f;
    [Export] public float RotationTimerStart = 0.3f;

    public float RotationStart = 0f;
    public float RotationEnd = 0f;
    public float RotationTimer;

    public Timer ShootTimer;
    public Node2D RotateGroup;
    public Area2D TargetArea;
    public AnimationPlayer TopAnimationPlayer;

    // Shake
    [Export] public float ShakeDuration = .1f;
    [Export] public float ShakeFrequency = 50f;
    [Export] public float ShakeAmplitude = 0.5f;

    public override void _Ready()
    {
        ShootTimer = GetNode<Timer>("ShootTimer");
        RotateGroup = GetNode<Node2D>("Rotate");
        TargetArea = GetNode<Area2D>("TargetArea");
        TopAnimationPlayer = GetNode<AnimationPlayer>("Rotate/Top/AnimationPlayer");

        base._Ready();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        TargetArea.Visible = Globals.HoveringBuilding == this || Globals.SelectedBuilding == this;

        if (TargetArea.GetOverlappingBodies().Count == 0)
        {
            ShootTimer.Stop();

            RotationTimer = RotationTimerStart;
        }
        else
        {
            if (ShootTimer.IsStopped())
            {
                ShootTimer.Start();
            }

            PhysicsBody2D closestTarget = (PhysicsBody2D)TargetArea.GetOverlappingBodies()[0];
            float closestDistance = GlobalPosition.DistanceTo(closestTarget.GlobalPosition);

            foreach (PhysicsBody2D body in TargetArea.GetOverlappingBodies())
            {
                float dist = GlobalPosition.DistanceTo(body.GlobalPosition);
                if (closestDistance > dist)
                {
                    closestTarget = body;
                    closestDistance = dist;
                }
            }

            float target = closestTarget.GlobalPosition.AngleToPoint(RotateGroup.GlobalPosition);

            if (target != RotationEnd)
            {
                RotationStart = RotateGroup.GlobalRotation;
                RotationEnd = target;
                RotationTimer = RotationTimerStart;
            }

            if (RotateGroup.GlobalRotation != RotationEnd)
            {
                RotateGroup.GlobalRotation = Mathf.LerpAngle(RotateGroup.GlobalRotation, RotationEnd, RotationTimer);

                RotationTimer += delta * RotationSpeed;

                RotationTimer = Mathf.Min(RotationTimer, 1f);
            }
        }
    }

    public void _OnTimeout()
    {
        bool canShoot = true;

        foreach (Item item in Uses.Keys)
        {
            if (Items[item] < Uses[item])
            {
                canShoot = false;
            }
        }

        if (canShoot)
        {
            foreach (Item item in Uses.Keys)
            {
                Items[item] -= Uses[item];
            }

            Shoot();
        }
    }

    public override void Tick()
    {
        base.Tick();

        bool canShoot = true;

        foreach (Item item in Uses.Keys)
        {
            if (Items[item] < Uses[item])
            {
                canShoot = false;
            }
        }

        WarningSprite.Visible = !canShoot;
    }

    public virtual void Shoot()
    {
        TopAnimationPlayer.Play("Shoot");
        TopAnimationPlayer.Seek(0, true);

        GetTree().CallGroup(
            "ShakeCamera", "Shake",
            ShakeDuration,
            ShakeFrequency,
            ShakeAmplitude
        );
    }
}
