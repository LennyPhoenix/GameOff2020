using Godot;

public class MinimapIcon : Node2D
{
    [Export] public MinimapIconType IconType = MinimapIconType.Core;

    [Export] public NodePath Root;

    [Export] public bool HideOutsideMap = true;

    public Sprite Marker;

    public override void _ExitTree()
    {
        base._ExitTree();

        if (IsInstanceValid(Marker))
        {
            Marker.QueueFree();
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (!IsInstanceValid(Marker))
        {
            if (Globals.Minimap != null)
            {
                Marker = Globals.Minimap.AddMarker(IconType);
            }
        }
        else
        {
            Vector2 objPosition = (GetNode<Node2D>(Root).GlobalPosition - Globals.Minimap.Player.GlobalPosition) / Globals.TileSize + Globals.Minimap.Markers.RectSize / 2;

            if (HideOutsideMap)
            {
                Marker.Visible = Globals.Minimap.Markers.GetRect().HasPoint(objPosition);
            }

            objPosition.x = Mathf.Clamp(objPosition.x, 0, Globals.Minimap.Markers.RectSize.x);
            objPosition.y = Mathf.Clamp(objPosition.y, 0, Globals.Minimap.Markers.RectSize.y);

            Marker.Position = objPosition;
            Marker.Rotation = GetNode<Node2D>(Root).GlobalRotation;
        }
    }
}
