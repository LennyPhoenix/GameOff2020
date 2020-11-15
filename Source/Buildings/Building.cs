using Godot;
using Godot.Collections;

public class Building : StaticBody2D
{
    [Export] public PackedScene PipeScene;

    [Export] public Dictionary<int, int> MaxStorage = new Dictionary<int, int>();
    [Export] public Dictionary<int, int> Outputs = new Dictionary<int, int>();

    [Export] public int MaxInput = 4;
    [Export] public int MaxOutput = 4;

    [Export] public Dictionary<int, int> Takes = new Dictionary<int, int>();
    [Export] public Dictionary<int, int> Produces = new Dictionary<int, int>();

    public Pipe DraggingPipe;
    public Sprite InputConnectionHighlight;
    public Sprite OutputConnectionHighlight;

    public Array<Building> InputBuildings = new Array<Building>();
    public Array<Building> OutputBuildings = new Array<Building>();
    public Dictionary<Building, Pipe> OutputPipes = new Dictionary<Building, Pipe>();

    public Node2D Pipes;

    private bool hovering = false;

    public override void _Ready()
    {
        base._Ready();

        Pipes = GetTree().CurrentScene.GetNode<Node2D>("Planet/Pipes");
        InputConnectionHighlight = GetNode<Sprite>("Highlights/InputConnection");
        OutputConnectionHighlight = GetNode<Sprite>("Highlights/OutputConnection");
    }

    public void _OnMouseEntered()
    {
        hovering = true;
        Globals.HoveringBuilding = this;

        foreach (Building output in OutputBuildings)
        {
            output.InputConnectionHighlight.Show();
        }

        foreach (Building input in InputBuildings)
        {
            input.OutputConnectionHighlight.Show();
        }
    }

    public void _OnMouseExited()
    {
        hovering = false;
        if (Globals.HoveringBuilding == this)
        {
            Globals.HoveringBuilding = null;
        }

        foreach (Building output in OutputBuildings)
        {
            output.InputConnectionHighlight.Hide();
        }

        foreach (Building input in InputBuildings)
        {
            input.OutputConnectionHighlight.Hide();
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

    public void Destroy()
    {
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
}
