using Godot;
using Godot.Collections;

public class Building : StaticBody2D
{
    [Export] public PackedScene PipeScene;
    public PackedScene PylonScene;

    [Export] public Dictionary<Item, int> MaxStorage = new Dictionary<Item, int>();
    [Export] public Dictionary<Item, int> Outputs = new Dictionary<Item, int>();

    [Export] public int MaxInput = 4;
    [Export] public int MaxOutput = 4;

    public AnimationPlayer AnimationPlayer;
    public Pipe DraggingPipe;
    public Sprite InputConnectionHighlight;
    public Sprite OutputConnectionHighlight;
    public Sprite WarningSprite;

    public Dictionary<Item, int> Items = new Dictionary<Item, int>();

    public Array<Building> InputBuildings = new Array<Building>();
    public Array<Building> OutputBuildings = new Array<Building>();
    public Dictionary<Building, Pipe> OutputPipes = new Dictionary<Building, Pipe>();

    public Node2D Pipes;

    public bool Deleting = false;

    private bool hovering = false;

    public override void _Ready()
    {
        base._Ready();

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Pipes = GetTree().CurrentScene.GetNode<Node2D>("Planet/Pipes");
        InputConnectionHighlight = GetNode<Sprite>("Highlights/InputConnection");
        OutputConnectionHighlight = GetNode<Sprite>("Highlights/OutputConnection");
        WarningSprite = GetNode<Sprite>("Warning");

        PylonScene = ResourceLoader.Load<PackedScene>("res://Source/Buildings/Pylon.tscn");

        foreach (Item item in (Item[])System.Enum.GetValues(typeof(Item)))
        {
            Items.Add(item, 0);
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

            if (Input.IsActionJustReleased("build") || (Globals.BuildMode && Globals.BuildBlueprint != null))
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
                    Vector2 position = new Vector2();

                    position.x = Mathf.FloorToInt(mousePos.x / Globals.TileSize) * Globals.TileSize + 8;
                    position.y = Mathf.FloorToInt(mousePos.y / Globals.TileSize) * Globals.TileSize + 8;

                    var pylon = (Pylon)PylonScene.Instance();
                    pylon.GlobalPosition = position;
                    GetParent<Node>().AddChild(pylon);
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
        else if (Input.IsActionJustReleased("build") && Globals.HoveringBuilding == this && AnimationPlayer.CurrentAnimation != "Spawn" && (!Globals.BuildMode || Globals.BuildBlueprint == null))
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

        if (hovering && Input.IsActionJustPressed("build") && OutputBuildings.Count < MaxOutput && (!Globals.BuildMode || Globals.BuildBlueprint == null))
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
            if (Globals.SelectedBuilding != null)
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
    }

    public void Destroy()
    {
        Deleting = true;
        SetSelected(false);

        if (Globals.HoveringBuilding == this)
        {
            Globals.HoveringBuilding = null;
        }

        if (DraggingPipe != null)
        {
            DraggingPipe.QueueFree();
        }

        foreach (Building output in OutputBuildings)
        {
            output.InputConnectionHighlight.Hide();
            output.InputBuildings.Remove(this);
            Pipe pipe = OutputPipes[output];
            pipe.PlayDelete();
        }

        foreach (Building input in InputBuildings)
        {
            input.OutputConnectionHighlight.Hide();
            input.OutputBuildings.Remove(this);
            Pipe pipe = input.OutputPipes[this];
            pipe.PlayDelete();
            input.OutputPipes.Remove(this);
        }

        AnimationPlayer.Play("Delete");
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
    }
}
