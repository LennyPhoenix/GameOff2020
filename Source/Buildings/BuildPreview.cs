using Godot;

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
            Rect.Extents = Blueprint.Size * Globals.TileSize / 2 - new Vector2(1, 1);
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
        Rect.Extents = Blueprint.Size * Globals.TileSize / 2 - new Vector2(1, 1);
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

        if (Colliding)
        {
            AnimationPlayer.Play("Obstructed");
        }
        else
        {
            AnimationPlayer.Play("Placeable");
        }

        Vector2 mousePos = GetGlobalMousePosition();
        Vector2 position = new Vector2();

        if (Mathf.RoundToInt(Blueprint.Size.x) % 2 == 1)
        {
            position.x = Mathf.FloorToInt(mousePos.x / Globals.TileSize) * Globals.TileSize + 8;
        }
        else
        {
            position.x = Mathf.RoundToInt(mousePos.x / Globals.TileSize) * Globals.TileSize;
        }

        if (Mathf.RoundToInt(Blueprint.Size.y) % 2 == 1)
        {
            position.y = Mathf.FloorToInt(mousePos.y / Globals.TileSize) * Globals.TileSize + 8;
        }
        else
        {
            position.y = Mathf.RoundToInt(mousePos.y / Globals.TileSize) * Globals.TileSize;
        }

        GlobalPosition = position;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event.IsActionPressed("place") && !Colliding && Visible)
        {
            var building = (Node2D)Blueprint.Scene.Instance();
            building.GlobalPosition = GlobalPosition;
            Buildings.AddChild(building);
        }
    }

    public void ToggleEnabled()
    {
        Visible = !Visible;
    }
}
