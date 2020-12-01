using Godot;
using Godot.Collections;

public class Building : StaticBody2D
{
    [Export] public PackedScene PipeScene;
    [Export] public PackedScene StorageItemScene;
    public Blueprint PylonBlueprint;

    [Export] public Dictionary<Item, int> MaxStorage = new Dictionary<Item, int>();
    [Export] public Dictionary<Item, int> Outputs = new Dictionary<Item, int>();

    [Export] public int MaxInput = 4;
    [Export] public int MaxOutput = 4;

    [Export] public int Size = 3;

    [Export] public bool BlockNavigation = true;

    public AudioStreamPlayer2D Hit;
    public AnimationPlayer AnimationPlayer;
    public AnimationPlayer SpriteAnimationPlayer;
    public Pipe DraggingPipe;
    public Sprite InputConnectionHighlight;
    public Sprite OutputConnectionHighlight;
    public Sprite WarningSprite;

    public Control UI;
    public AnimationPlayer UIAnimationPlayer;
    public PanelContainer StorageContainer;
    public GridContainer StorageGridContainer;
    public Label UIInputLabel;
    public Label UIOutputLabel;
    public HealthBar HealthBar;

    public Dictionary<Item, int> Items = new Dictionary<Item, int>();

    public Array<Building> InputBuildings = new Array<Building>();
    public Array<Building> OutputBuildings = new Array<Building>();
    public Dictionary<Building, Pipe> OutputPipes = new Dictionary<Building, Pipe>();
    public Dictionary<Item, StorageItem> UIStorageItems = new Dictionary<Item, StorageItem>();

    public Node2D Pipes;
    public Node2D BuildingUI;
    public TileMap NavMap;

    public bool Deleting = false;

    private bool hovering = false;

    public override void _Ready()
    {
        base._Ready();

        PylonBlueprint = ResourceLoader.Load<Blueprint>("res://Assets/Buildings/Pylon.tres");

        Hit = GetNode<AudioStreamPlayer2D>("Hit");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        SpriteAnimationPlayer = GetNode<AnimationPlayer>("Sprite/AnimationPlayer");
        InputConnectionHighlight = GetNode<Sprite>("Highlights/InputConnection");
        OutputConnectionHighlight = GetNode<Sprite>("Highlights/OutputConnection");
        WarningSprite = GetNode<Sprite>("Warning");

        UI = GetNode<Control>("UI");
        UIAnimationPlayer = GetNode<AnimationPlayer>("UI/AnimationPlayer");
        StorageContainer = GetNode<PanelContainer>("UI/StorageContainer");
        StorageGridContainer = GetNode<GridContainer>("UI/StorageContainer/GridContainer");
        UIInputLabel = GetNode<Label>("UI/ConnectionContainer/VBoxContainer/Input/Label");
        UIOutputLabel = GetNode<Label>("UI/ConnectionContainer/VBoxContainer/Output/Label");
        HealthBar = GetNode<HealthBar>("UI/HealthBar");

        Pipes = GetTree().CurrentScene.GetNode<Node2D>("Planet/Pipes");
        BuildingUI = GetTree().CurrentScene.GetNode<Node2D>("Planet/BuildingUI");
        NavMap = GetTree().CurrentScene.GetNode<TileMap>("Planet/NavigationManager/Map");

        RemoveChild(UI);
        BuildingUI.AddChild(UI);
        UI.RectGlobalPosition = GlobalPosition;
        UI.Name = Name;

        UIInputLabel.Text = InputBuildings.Count + "/" + MaxInput;
        UIOutputLabel.Text = OutputBuildings.Count + "/" + MaxOutput;

        foreach (Item item in (Item[])System.Enum.GetValues(typeof(Item)))
        {
            Items.Add(item, 0);
        }

        int columns = Mathf.Min(4, MaxStorage.Count);
        int rows = Mathf.CeilToInt((float)MaxStorage.Count / 4);
        StorageContainer.RectSize = new Vector2(columns * 16 + 2 + Mathf.Min(columns-1, 0), rows * 16 + 2 + Mathf.Min(rows - 1, 0));
        StorageGridContainer.Columns = Mathf.Max(columns, 1);

        foreach (Item item in MaxStorage.Keys)
        {
            var storageItem = (StorageItem)StorageItemScene.Instance();
            StorageGridContainer.AddChild(storageItem);
            storageItem.ItemType = item;
            storageItem.Count = Items[item];

            UIStorageItems.Add(item, storageItem);
        }

        Globals.LastBuilding = this;

        if (BlockNavigation)
        {
            Array<Vector2> covers = GetCoveredTiles();
            foreach (Vector2 pos in covers)
            {
                int x = Mathf.RoundToInt(pos.x);
                int y = Mathf.RoundToInt(pos.y);
                NavMap.SetCell(x, y, -1);
                NavMap.UpdateBitmaskArea(new Vector2(x, y));
            }

            if (IsInGroup("EnemyTargets"))
            {
                GetTree().CallGroup("Enemies", "RecalculatePath");
            }
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (Deleting)
        {
            return;
        }

        if (DraggingPipe != null)
        {
            DraggingPipe.PointB = GetGlobalMousePosition();

            if (Input.IsActionJustReleased("build"))
            {
                if (Globals.HoveringBuilding != null && Globals.HoveringBuilding != this && DraggingPipe.CanPlace)
                {
                    Building building = Globals.HoveringBuilding;
                    if (!building.InputBuildings.Contains(this) && !InputBuildings.Contains(building) && building.InputBuildings.Count < building.MaxInput)
                    {
                        AddOutput(building);
                        OutputConnectionHighlight.Show();
                    }
                }
                else if (Globals.HoveringBuilding == null && DraggingPipe.CanPlace)
                {
                    Globals.LastBuilding = this;

                    Vector2 mousePos = GetGlobalMousePosition();

                    var pylon = (Pylon)Globals.BuildManager.BuildBuilding(PylonBlueprint);
                    if (pylon != null)
                    {
                        pylon.GlobalPosition = new Vector2
                        {
                            x = Mathf.FloorToInt(mousePos.x / Globals.TileSize) * Globals.TileSize + 8,
                            y = Mathf.FloorToInt(mousePos.y / Globals.TileSize) * Globals.TileSize + 8
                        };
                        Globals.BuildManager.Buildings.AddChild(pylon);
                    }
                }

                DraggingPipe.QueueFree();
                DraggingPipe = null;
                Globals.DraggingBuilding = null;
            }
        }

        if (Input.IsActionJustReleased("build") && Globals.HoveringBuilding != this && Globals.SelectedBuilding == this)
        {
            SetSelected(false);
        }
        else if (Input.IsActionJustReleased("build") && Globals.HoveringBuilding == this && Globals.SelectedBuilding != this && AnimationPlayer.CurrentAnimation != "Spawn")
        {
            SetSelected(true);
        }

        if (Input.IsActionJustReleased("deconstruct") && Globals.HoveringBuilding == this && Globals.SelectedBuilding != null)
        {
            Building building = Globals.SelectedBuilding;

            if (InputBuildings.Contains(building))
            {
                building.RemoveOutput(this);
                InputConnectionHighlight.Hide();
            }
            
            if (OutputBuildings.Contains(building))
            {
                RemoveOutput(building);
                OutputConnectionHighlight.Hide();
            }
        }

        if (hovering && Input.IsActionJustPressed("build") && OutputBuildings.Count < MaxOutput)
        {
            DraggingPipe = (Pipe)PipeScene.Instance();
            DraggingPipe.PointA = GlobalPosition;
            DraggingPipe.PointB = GetGlobalMousePosition();
            Pipes.AddChild(DraggingPipe);
            DraggingPipe.PlayPlacing();
            Globals.DraggingBuilding = this;
        }

        UI.RectGlobalPosition = GlobalPosition;
    }

    public void _OnMouseEntered()
    {
        if (Deleting)
        {
            return;
        }

        hovering = true;
        Globals.HoveringBuilding = this;

        if (Globals.SelectedBuilding == null)
        {
            foreach (Building output in OutputBuildings)
            {
                output.InputConnectionHighlight.Show();
            }

            foreach (Building input in InputBuildings)
            {
                input.OutputConnectionHighlight.Show();
            }

            UIAnimationPlayer.Play("Show");
        }
    }

    public void _OnMouseExited()
    {
        if (Deleting)
        {
            return;
        }

        hovering = false;
        if (Globals.HoveringBuilding == this)
        {
            Globals.HoveringBuilding = null;
        }

        if (Globals.SelectedBuilding == null)
        {
            foreach (Building output in OutputBuildings)
            {
                if (Globals.HoveringBuilding == null || !Globals.HoveringBuilding.OutputBuildings.Contains(output))
                {
                    output.InputConnectionHighlight.Hide();
                }
            }

            foreach (Building input in InputBuildings)
            {
                if (Globals.HoveringBuilding == null || !Globals.HoveringBuilding.InputBuildings.Contains(input))
                {
                    input.OutputConnectionHighlight.Hide();
                }
            }

            UIAnimationPlayer.Play("Hide");
        }
    }

    public void _OnHit(float newHealth, float maxHealth)
    {
        Hit.Play();

        if (Deleting)
        {
            return;
        }

        HealthBar.Health = newHealth / maxHealth;

        if (newHealth <= 0)
        {
            Destroy();

            AnimationPlayer.Play("Destroy");
        }
    }

    public void SetSelected(bool selected)
    {
        if (Deleting)
        {
            return;
        }

        if (selected)
        {
            if (Globals.SelectedBuilding != null && Globals.SelectedBuilding != this)
            {
                Globals.SelectedBuilding.SetSelected(false);
            }

            Globals.SelectedBuilding = this;
            AnimationPlayer.Play("Selected");

            foreach (Building output in OutputBuildings)
            {
                output.InputConnectionHighlight.Show();
            }

            foreach (Building input in InputBuildings)
            {
                input.OutputConnectionHighlight.Show();
            }

            if (UIAnimationPlayer.AssignedAnimation != "Show")
            {
                UIAnimationPlayer.Play("Show");
            }
        }
        else
        {
            Globals.SelectedBuilding = null;
            AnimationPlayer.Play("Default");

            foreach (Building output in OutputBuildings)
            {
                output.InputConnectionHighlight.Hide();
            }

            foreach (Building input in InputBuildings)
            {
                input.OutputConnectionHighlight.Hide();
            }

            if (UIAnimationPlayer.AssignedAnimation != "Hide")
            {
                UIAnimationPlayer.Play("Hide");
            }
        }
    }

    public void AddOutput(Building building)
    {
        if (Deleting || building.Deleting)
        {
            return;
        }

        Pipe pipe = (Pipe)PipeScene.Instance();
        pipe.PointA = GlobalPosition;
        pipe.PointB = building.GlobalPosition;
        Pipes.AddChild(pipe);

        OutputBuildings.Add(building);
        OutputPipes.Add(building, pipe);
        building.InputBuildings.Add(this);

        if (building.UIInputLabel != null)
        {
            building.UIInputLabel.Text = building.InputBuildings.Count + "/" + building.MaxInput;
        }
        UIOutputLabel.Text = OutputBuildings.Count + "/" + MaxOutput;
    }

    public void RemoveOutput(Building building)
    {
        Pipe pipe = OutputPipes[building];
        pipe.PlayDelete();
        OutputPipes.Remove(building);

        if (!Deleting)
        {
            OutputBuildings.Remove(building);
        }

        if (!building.Deleting)
        {
            building.InputBuildings.Remove(this);
        }

        if (building.UIInputLabel != null)
        {
            building.UIInputLabel.Text = building.InputBuildings.Count + "/" + building.MaxInput;
        }
        UIOutputLabel.Text = OutputBuildings.Count + "/" + MaxOutput;
    }

    public Array<Vector2> GetCoveredTiles()
    {
        var offset = new Vector2(Mathf.FloorToInt(Size / 2), Mathf.FloorToInt(Size / 2));
        Vector2 topLeft = (GlobalPosition / Globals.TileSize) - offset;

        var positions = new Array<Vector2>();

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                positions.Add(new Vector2(x + Mathf.FloorToInt(topLeft.x), y + Mathf.FloorToInt(topLeft.y)));
            }
        }

        return positions;
    }

    public virtual void Destroy()
    {
        SetSelected(false);

        if (Globals.HoveringBuilding == this)
        {
            Globals.HoveringBuilding = null;
        }

        if (Globals.LastBuilding == this)
        {
            Globals.LastBuilding = null;
        }

        if (DraggingPipe != null)
        {
            DraggingPipe.QueueFree();
        }

        Deleting = true;

        foreach (Building output in OutputBuildings)
        {
            RemoveOutput(output);
        }

        foreach (Building input in InputBuildings)
        {
            input.RemoveOutput(this);
        }

        UI.QueueFree();

        AnimationPlayer.Play("Delete");

        if (BlockNavigation)
        {
            Array<Vector2> covers = GetCoveredTiles();
            foreach (Vector2 pos in covers)
            {
                int x = Mathf.RoundToInt(pos.x);
                int y = Mathf.RoundToInt(pos.y);
                NavMap.SetCell(x, y, 0);
                NavMap.UpdateBitmaskArea(new Vector2(x, y));
            }

            if (IsInGroup("EnemyTargets"))
            {
                GetTree().CallGroup("Enemies", "RecalculatePath");
            }
        }
    }

    public virtual void Tick() {
        if (Deleting)
        {
            return;
        }

        foreach (Building output in OutputBuildings)
        {
            foreach (Item item in Outputs.Keys)
            {
                if (output.MaxStorage.ContainsKey(item))
                {
                    int num = Outputs[item];

                    int send = Mathf.Min(num, Items[item]);
                    send = Mathf.Min(send, output.MaxStorage[item] - output.Items[item]);

                    output.Items[item] += send;
                    Items[item] -= send;
                }
            }
        }

        foreach (Item item in MaxStorage.Keys)
        {
            if (UIStorageItems.ContainsKey(item))
            {
                StorageItem storageItem = UIStorageItems[item];
                storageItem.Count = Items[item];
            }
        }
    }
}
