using Godot;

public class MinimapIcon : Node2D
{
    [Export] public MinimapIconType IconType = MinimapIconType.Core;

    [Export] public NodePath Root;

    [Export] public bool HideOutsideMap = true;
}
