using Godot;

public class Pylon : GenericCarrier
{
    public override void _Ready()
    {
        Building lastBuilding = Globals.LastBuilding;

        base._Ready();

        if (lastBuilding != null && lastBuilding.GlobalPosition.DistanceTo(GlobalPosition) < Pipe.MaxLength * Globals.TileSize)
        {
            lastBuilding.AddOutput(this);
        }
    }
}
