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
    public Sprite DeconstructSprite;
    public AnimationPlayer DeconstructAnimationPlayer;
    public CollisionShape2D Collider;
    public Node2D Buildings;

    public bool BuildMode
    {
        get => Sprite.Visible;
        set { Sprite.Visible = value; }
    }
    public bool DeconstructMode
    {
        get => DeconstructSprite.Visible;
        set {
            Globals.DeconstructMode = value;
            DeconstructSprite.Visible = value;
            if (value && !(Globals.SelectedBuilding is null))
            {
                Globals.SelectedBuilding.SetSelected(false);
            }
            else if (!value)
            {
                DeconstructAnimationPlayer.Play("Spin");
            }
        }
    }

    private Building deconstructBuilding;

    public override void _Ready()
    {
        base._Ready(); 

        if (Engine.EditorHint)
        {
            return;
        }

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Sprite = GetNode<Sprite>("Sprite");
        DeconstructSprite = GetNode<Sprite>("DeconstructSprite");
        DeconstructAnimationPlayer = GetNode<AnimationPlayer>("DeconstructSprite/AnimationPlayer");
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
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (Engine.EditorHint)
        {
            return;
        }

        if (Input.IsActionJustPressed("build_mode"))
        {
            DeconstructMode = false;
            deconstructBuilding = null;

            BuildMode = !BuildMode;
        }

        if (Input.IsActionJustPressed("deconstruct_mode"))
        {
            DeconstructMode = !DeconstructMode;
            deconstructBuilding = null;

            BuildMode = false;
        }

        Vector2 mousePos = GetGlobalMousePosition();
        if (DeconstructMode)
        {
            if (Globals.HoveringBuilding is null || Globals.HoveringBuilding.Name == "Core")
            {
                deconstructBuilding = null;
                GlobalPosition = mousePos;
                if (DeconstructAnimationPlayer.CurrentAnimation == "Delete")
                {
                    DeconstructAnimationPlayer.Play("Spin");
                }
            }
            else
            {
                GlobalPosition = Globals.HoveringBuilding.GlobalPosition;

                if (deconstructBuilding != Globals.HoveringBuilding && Input.IsActionPressed("select"))
                {
                    deconstructBuilding = Globals.HoveringBuilding;
                    DeconstructAnimationPlayer.Play("Delete");
                    DeconstructAnimationPlayer.Seek(0, true);
                }
                else if (!Input.IsActionPressed("select") && DeconstructAnimationPlayer.CurrentAnimation == "Delete")
                {
                    DeconstructAnimationPlayer.Play("Spin");
                    deconstructBuilding = null;
                }
            }
        }
        else
        {
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
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (Engine.EditorHint)
        {
            return;
        }

        if (@event.IsActionPressed("select") && !Colliding && BuildMode)
        {
            var building = (Node2D)Blueprint.Scene.Instance();
            building.GlobalPosition = GlobalPosition;
            Buildings.AddChild(building);
        }
    }

    public void _OnDeconstructAnimationFinished(string animName)
    {
        if (animName == "Delete")
        {
            if (deconstructBuilding == Globals.HoveringBuilding)
            {
                deconstructBuilding.Destroy();
                deconstructBuilding = null;
                DeconstructAnimationPlayer.Play("Deleted");
            }
        }
        else if (animName == "Deleted")
        {
            DeconstructAnimationPlayer.Play("Spin");
        }
    }
}
