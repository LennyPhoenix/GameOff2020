using Godot;
using System;

public class Pipe : Sprite
{
    private Vector2 pointA = new Vector2(0, 0);
    [Export] public Vector2 PointA
    {
        get => pointA;
        set
        {
            pointA = value;
            UpdatePosition();
        }
    }

    private Vector2 pointB = new Vector2(64, 0);
    [Export] public Vector2 PointB
    {
        get => pointB;
        set
        {
            pointB = value;
            UpdatePosition();
        }
    }

    public override void _Ready()
    {
        base._Ready();

        UpdatePosition();
    }

    public void UpdatePosition()
    {
        Position = PointA;
        Rotation = (PointB - PointA).Angle();
        RegionRect = new Rect2(0, 0, PointA.DistanceTo(PointB), RegionRect.Size.y);
    }
}
