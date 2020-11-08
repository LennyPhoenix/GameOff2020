using Godot;
using System;

[Tool]
public class MapWrapper : Node
{
    [Export] public NodePath RootPath;
    [Export] private NodePath CameraPath;

    public override string _GetConfigurationWarning()
    {
        if (RootPath is null)
        {
            return "RootPath property is empty.";
        }
        return "";
    }

    public void Wrap(int PerimeterSize, int WorldSize)
    {
        Node2D root = GetNode<Node2D>(RootPath);

        Vector2 newPos = root.GlobalPosition;
        newPos.x = (newPos.x + PerimeterSize) % WorldSize;
        newPos.y = (newPos.y + PerimeterSize) % WorldSize;

        if (newPos.Round() != root.GlobalPosition.Round())
        {
            root.GlobalPosition = newPos;
            if (!(CameraPath is null))
            {
                Camera2D camera = GetNode<Camera2D>(CameraPath);
                camera.ForceUpdateScroll();
                camera.ResetSmoothing();
            }
        }
    }
}
