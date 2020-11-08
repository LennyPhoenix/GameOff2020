using Godot;
using System;

[Tool]
public class MapWrapper : Node2D
{
    [Export] public NodePath RootPath = "";
    [Export] public NodePath CameraPath = "";

    [Export] public Godot.Collections.Array<NodePath> AdditionalOffsetPaths = new Godot.Collections.Array<NodePath>();

    public override string _GetConfigurationWarning()
    {
        if (RootPath == "")
        {
            return "RootPath property is empty.";
        }
        return "";
    }

    public void Update(int perimeterSize, int worldSize, Vector2 cameraPosition)
    {
        Wrap(perimeterSize, worldSize);
        OffsetObjects(worldSize, cameraPosition);
    }

    public void Wrap(int perimeterSize, int worldSize)
    {
        Node2D root = GetNode<Node2D>(RootPath);

        Vector2 newPos = root.GlobalPosition;
        newPos.x = (newPos.x + perimeterSize) % worldSize;
        newPos.y = (newPos.y + perimeterSize) % worldSize;

        if (newPos.Round() != root.GlobalPosition.Round())
        {
            root.GlobalPosition = newPos;
            if (CameraPath != "")
            {
                Camera2D camera = GetNode<Camera2D>(CameraPath);
                camera.ForceUpdateScroll();
                camera.ResetSmoothing();
            }
        }
    }

    public void OffsetObjects(int worldSize, Vector2 cameraPosition)
    {
        Node2D root = GetNode<Node2D>(RootPath);

        Vector2 offset = new Vector2();

        // X Position
        if (
            cameraPosition.x < worldSize / 4
            && root.GlobalPosition.x > worldSize / 4 * 3 
        )
        {
            offset.x = -worldSize;
        }
        else if (
            cameraPosition.x > worldSize / 4 * 3
            && root.GlobalPosition.x < worldSize / 4
        )
        {
            offset.x = worldSize;
        }
        else
        {
            offset.x = 0;
        }

        // Y Position
        if (
            cameraPosition.y < worldSize / 4
            && root.GlobalPosition.y > worldSize / 4 * 3
        )
        {
            offset.y = -worldSize;
        }
        else if (
            cameraPosition.y > worldSize / 4 * 3
            && root.GlobalPosition.y < worldSize / 4
        )
        {
            offset.y = worldSize;
        }
        else
        {
            offset.y = 0;
        }

        GlobalPosition = offset;
        foreach (NodePath nodePath in AdditionalOffsetPaths)
        {
            if (nodePath == "")
            {
                continue;
            }
            Node2D node = GetNode<Node2D>(nodePath);
            node.GlobalPosition = root.GlobalPosition + offset;
        }
    }
}
