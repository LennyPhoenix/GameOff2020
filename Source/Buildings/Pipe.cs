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

    [Export] public Material PlaceMaterial;

    public static int MaxLength = 24;

    public AnimationPlayer AnimationPlayer;

    public bool CanPlace
    {
        get
        {
            return PointA.DistanceTo(PointB) < MaxLength * Globals.TileSize;
        }
    }
    private bool placing = false;

    public override void _Ready()
    {
        base._Ready();

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        UpdatePosition();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (placing)
        {
            AnimationPlayer.Play(CanPlace ? "Placing" : "CannotPlace");
        }
    }

    public void UpdatePosition()
    {
        GlobalPosition = PointA;
        GlobalRotation = (PointB - PointA).Angle();
        RegionRect = new Rect2(0, 0, PointA.DistanceTo(PointB), RegionRect.Size.y);
    }

    public void PlayPlacing()
    {
        placing = true;
        AnimationPlayer.Play("Placing");
        Material = PlaceMaterial;
    }

    public void PlayDelete()
    {
        AnimationPlayer.Play("Delete");
    }
}
