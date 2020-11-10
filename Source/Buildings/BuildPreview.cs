using Godot;
using System;

[Tool]
public class BuildPreview : Area2D
{
    private Blueprint blueprint;
    [Export] public Blueprint Blueprint
    {
        get => blueprint;
        set
        {
            blueprint = value;

            if (Engine.EditorHint || Sprite is null)
            {
                return;
            }

            Sprite.Texture = value.BuildTexture;
            var Rect = new RectangleShape2D();
            Rect.Extents = Blueprint.Size / 2;
            Collider.Shape = Rect;
        }
    }

    public bool Colliding
    {
        get {
            return GetOverlappingBodies().Count != 0; 
        }
    }

    public AnimationPlayer AnimationPlayer;
    public Sprite Sprite;
    public CollisionShape2D Collider;
    public Node2D Buildings;

    public override void _Ready()
    {
        base._Ready(); 

        if (Engine.EditorHint)
        {
            return;
        }

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Sprite = GetNode<Sprite>("Sprite");
        Collider = GetNode<CollisionShape2D>("Collider");
        Buildings = GetNode<Node2D>("../Buildings");

        Sprite.Texture = Blueprint.BuildTexture;
        var Rect = new RectangleShape2D();
        Rect.Extents = Blueprint.Size / 2;
        Collider.Shape = Rect;
    }

    public override string _GetConfigurationWarning()
    {
        if (Blueprint is null)
        {
            return "Blueprint property is empty.";
        }
        return "";
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (Engine.EditorHint)
        {
            return;
        }

        if (Input.IsActionJustPressed("shoot") && !Colliding)
        {
            var building = (Node2D)Blueprint.Scene.Instance();
            building.GlobalPosition = GlobalPosition;
            Buildings.AddChild(building);
        }

        if (Colliding)
        {
            AnimationPlayer.Play("Obstructed");
        }
        else
        {
            AnimationPlayer.Play("Placeable");
        }

        Vector2 mousePos = GetGlobalMousePosition();
        GlobalPosition = new Vector2(
            Mathf.RoundToInt(mousePos.x / Globals.TileSize) * Globals.TileSize,
            Mathf.RoundToInt(mousePos.y / Globals.TileSize) * Globals.TileSize
        );
    }
}
