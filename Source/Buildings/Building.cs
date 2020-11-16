using Godot;
using Godot.Collections;

public class Building : StaticBody2D
{
    [Export] public PackedScene PipeScene;

    [Export] public Dictionary<Item, int> MaxStorage = new Dictionary<Item, int>();
    [Export] public Dictionary<Item, int> Outputs = new Dictionary<Item, int>();

    [Export] public int MaxInput = 4;
    [Export] public int MaxOutput = 4;

    public AnimationPlayer AnimationPlayer;
    public Pipe DraggingPipe;
    public Sprite InputConnectionHighlight;
    public Sprite OutputConnectionHighlight;

    public Dictionary<Item, int> Items = new Dictionary<Item, int>();

    public Array<Building> InputBuildings = new Array<Building>();
    public Array<Building> OutputBuildings = new Array<Building>();
    public Dictionary<Building, Pipe> OutputPipes = new Dictionary<Building, Pipe>();

    public Node2D Pipes;

    private bool hovering = false;

    public override void _Ready()
    {
        base._Ready();

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Pipes = GetTree().CurrentScene.GetNode<Node2D>("Planet/Pipes");
        InputConnectionHighlight = GetNode<Sprite>("Highlights/InputConnection");
        OutputConnectionHighlight = GetNode<Sprite>("Highlights/OutputConnection");

        foreach (Item item in (Item[])System.Enum.GetValues(typeof(Item)))
        {
            Items.Add(item, 0);
        }
    }

    public void _OnMouseEntered()
    {
        hovering = true;
        Globals.HoveringBuilding = this;

        if (Globals.SelectedBuilding is null)
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
        hovering = false;
        if (Globals.HoveringBuilding == this)
        {
            Globals.HoveringBuilding = null;
        }

        if (Globals.SelectedBuilding is null)
        {
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

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (!(DraggingPipe is null))
        {
            DraggingPipe.PointB = GetGlobalMousePosition();

            if (Input.IsActionJustReleased("select"))
            {
                DraggingPipe.QueueFree();
                DraggingPipe = null;
                Globals.DraggingBuilding = null;

                if (!(Globals.HoveringBuilding is null) && Globals.HoveringBuilding != this)
                {
                    Building building = Globals.HoveringBuilding;
                    if (!building.InputBuildings.Contains(this) && !InputBuildings.Contains(building) && building.InputBuildings.Count < building.MaxInput)
                    {
                        AddOutput(building);
                        OutputConnectionHighlight.Show();
                    }
                }
            }
        }

        if (Input.IsActionJustReleased("select") && Globals.HoveringBuilding != this && Globals.SelectedBuilding == this)
        {
            SetSelected(false);
        }
        else if (Input.IsActionJustReleased("select") && Globals.HoveringBuilding == this && AnimationPlayer.CurrentAnimation != "Spawn")
        {
            SetSelected(true);
        }

        if (Input.IsActionJustReleased("remove_connections") && Globals.HoveringBuilding == this && !(Globals.SelectedBuilding is null))
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

        if (hovering && Input.IsActionJustPressed("select") && OutputBuildings.Count < MaxOutput)
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
        if (selected)
        {
            if (!(Globals.SelectedBuilding is null))
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
        Pipe pipe = OutputPipes[building];
        pipe.QueueFree();
        OutputPipes.Remove(building);
        OutputBuildings.Remove(building);
        building.InputBuildings.Remove(this);
    }

    public void Destroy()
    {
        SetSelected(false);

        foreach (Building output in OutputBuildings)
        {
            output.InputBuildings.Remove(this);
            Pipe pipe = OutputPipes[output];
            pipe.QueueFree();
        }

        foreach (Building input in InputBuildings)
        {
            input.OutputBuildings.Remove(this);
            Pipe pipe = input.OutputPipes[this];
            pipe.QueueFree();
            input.OutputPipes.Remove(this);
        }

        QueueFree();
    }

    public virtual void Tick() {
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

        GD.Print(Items);
    }
}
