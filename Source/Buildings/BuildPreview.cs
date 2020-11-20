using Godot;
using Godot.Collections;

public class BuildPreview : Area2D
{
	private Blueprint blueprint;
	[Export] public Blueprint Blueprint
	{
		get => blueprint;
		set
		{
			blueprint = value;

			if (Sprite == null)
			{
				return;
			}

			if (value == null)
			{
				Sprite.Texture = null;
				var rect = new RectangleShape2D();
				rect.Extents = Vector2.Zero;
				Collider.Shape = rect;
			}
			else
			{
				Sprite.Texture = value.BuildTexture;
				var rect = new RectangleShape2D();
				rect.Extents = value.Size * Globals.TileSize / 2 - new Vector2(1, 1);
				Collider.Shape = rect;
			}
		}
	}

	[Export] public Dictionary<string, Array<Blueprint>> Blueprints = new Dictionary<string, Array<Blueprint>>();

	[Export] public PackedScene MenuItemScene;

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

	public NinePatchRect Border;

	public MarginContainer BuildMenu;
	public TabContainer MenuTabContainer;
	public MarginContainer CategoryContainer;

	private bool enabled = false;
	public bool Enabled
	{
		get => enabled;
		set {
			enabled = value;
			Globals.BuildMode = value;
			Border.Visible = value;
			BuildMenu.Visible = value;
			Globals.LastBuilding = null;

			if (Globals.SelectedBuilding != null)
			{
				Globals.SelectedBuilding.SetSelected(false);
			}
		}
	}

	private Building deconstructBuilding;

	public override void _Ready()
	{
		base._Ready();

		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		Sprite = GetNode<Sprite>("Sprite");
		DeconstructSprite = GetNode<Sprite>("DeconstructSprite");
		DeconstructAnimationPlayer = GetNode<AnimationPlayer>("DeconstructSprite/AnimationPlayer");
		Collider = GetNode<CollisionShape2D>("Collider");
		Buildings = GetNode<Node2D>("../Buildings");

		Border = GetNode<NinePatchRect>("UI/Border");

		BuildMenu = GetNode<MarginContainer>("UI/BuildMenu");
		MenuTabContainer = GetNode<TabContainer>("UI/BuildMenu/TabContainer");
		CategoryContainer = GetNode<MarginContainer>("UI/BuildMenu/CategoryContainer");

		if (Blueprint == null)
		{
			Sprite.Texture = null;
			var rect = new RectangleShape2D();
			rect.Extents = Vector2.Zero;
			Collider.Shape = rect;
		}
		else
		{
			Sprite.Texture = Blueprint.BuildTexture;
			var rect = new RectangleShape2D();
			rect.Extents = Blueprint.Size * Globals.TileSize / 2 - new Vector2(1, 1);
			Collider.Shape = rect;
		}

		foreach (string categoryName in Blueprints.Keys)
		{
			Array<Blueprint> blueprints = Blueprints[categoryName];
			var categoryContainer = (MarginContainer)CategoryContainer.Duplicate();
			var gridContainer = categoryContainer.GetNode<GridContainer>("GridContainer");
			categoryContainer.Name = categoryName;
			categoryContainer.Visible = true;
			MenuTabContainer.AddChild(categoryContainer);

			foreach (Blueprint blueprint in blueprints)
			{
				var item = (BuildMenuItem)MenuItemScene.Instance();
				item.Blueprint = blueprint;
				item.BuildPreview = this;
				gridContainer.AddChild(item);
			}
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);

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

		if (Input.IsActionJustPressed("build_mode"))
		{
			Enabled = !Enabled;
			deconstructBuilding = null;
		}

		Vector2 mousePos = GetGlobalMousePosition();
		if (Input.IsActionPressed("deconstruct"))
		{
			if (Globals.HoveringBuilding == null || Globals.HoveringBuilding.Name == "Core")
			{
				deconstructBuilding = null;
				if (DeconstructAnimationPlayer.CurrentAnimation != "Deleted")
				{
					DeconstructAnimationPlayer.Play("Spin");
				}
				GlobalPosition = mousePos;
			}
			else
			{
				GlobalPosition = Globals.HoveringBuilding.GlobalPosition;

				if (deconstructBuilding != Globals.HoveringBuilding && Enabled)
				{
					deconstructBuilding = Globals.HoveringBuilding;
					DeconstructAnimationPlayer.Play("Delete");
					DeconstructAnimationPlayer.Seek(0, true);
				}
			}
		}
		else
		{
			deconstructBuilding = null;
			DeconstructAnimationPlayer.Play("Spin");

			if (Blueprint != null)
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

		Sprite.Visible = Enabled && !Input.IsActionPressed("deconstruct");
		DeconstructSprite.Visible = Enabled && Input.IsActionPressed("deconstruct");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		if (Blueprint == null)
		{
			return;
		}

		if (@event.IsActionPressed("build") && !Colliding && Enabled)
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
