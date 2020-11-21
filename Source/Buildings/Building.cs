using Godot;
using Godot.Collections;

public class Building : StaticBody2D
{
    public new Vector2 GlobalPosition
    {
        get => base.GlobalPosition;
        set
        {
            base.GlobalPosition = value;
            if (UI != null)
            {
                UI.RectGlobalPosition = value;
            }
        }
    }

    [Export] public PackedScene PipeScene;
    [Export] public PackedScene StorageItemScene;
    public Blueprint PylonBlueprint;

    [Export] public Dictionary<Item, int> MaxStorage = new Dictionary<Item, int>();
    [Export] public Dictionary<Item, int> Outputs = new Dictionary<Item, int>();

    [Export] public int MaxInput = 4;
    [Export] public int MaxOutput = 4;

    public AnimationPlayer AnimationPlayer;
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

    public Dictionary<Item, int> Items = new Dictionary<Item, int>();

    public Array<Building> InputBuildings = new Array<Building>();
    public Array<Building> OutputBuildings = new Array<Building>();
    public Dictionary<Building, Pipe> OutputPipes = new Dictionary<Building, Pipe>();
    public Dictionary<Item, StorageItem> UIStorageItems = new Dictionary<Item, StorageItem>();

    public Node2D Pipes;
    public Node2D BuildingUI;

    public bool Deleting = false;

    private bool hovering = false;

    public override void _Ready()
    {
        base._Ready();

        PylonBlueprint = ResourceLoader.Load<Blueprint>("res://Assets/Buildings/Pylon.tres");

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Pipes = GetTree().CurrentScene.GetNode<Node2D>("Planet/Pipes");
        BuildingUI = GetTree().CurrentScene.GetNode<Node2D>("Planet/BuildingUI");
        InputConnectionHighlight = GetNode<Sprite>("Highlights/InputConnection");
        OutputConnectionHighlight = GetNode<Sprite>("Highlights/OutputConnection");
        WarningSprite = GetNode<Sprite>("Warning");

        UI = GetNode<Control>("UI");
        UIAnimationPlayer = GetNode<AnimationPlayer>("UI/AnimationPlayer");
        StorageContainer = GetNode<PanelContainer>("UI/StorageContainer");
        StorageGridContainer = GetNode<GridContainer>("UI/StorageContainer/GridContainer");
        UIInputLabel = GetNode<Label>("UI/ConnectionContainer/VBoxContainer/Input/Label");
        UIOutputLabel = GetNode<Label>("UI/ConnectionContainer/VBoxContainer/Output/Label");

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
        StorageGridContainer.Columns = columns;

        foreach (Item item in MaxStorage.Keys)
        {
            var storageItem = (StorageItem)StorageItemScene.Instance();
            StorageGridContainer.AddChild(storageItem);
            storageItem.ItemType = item;
            storageItem.Count = Items[item];

            UIStorageItems.Add(item, storageItem);
        }

        Globals.LastBuilding = this;
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
                        AddOutput(pylon);
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
        if (Deleting || building.Deleting)
        {
            return;
        }

        Pipe pipe = OutputPipes[building];
        pipe.PlayDelete();
        OutputPipes.Remove(building);
        OutputBuildings.Remove(building);
        building.InputBuildings.Remove(this);

        if (building.UIInputLabel != null)
        {
            building.UIInputLabel.Text = building.InputBuildings.Count + "/" + building.MaxInput;
        }
        UIOutputLabel.Text = OutputBuildings.Count + "/" + MaxOutput;
    }

    public void Destroy()
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

        Deleting = true;
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
