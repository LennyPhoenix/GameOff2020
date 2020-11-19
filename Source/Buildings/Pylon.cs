using Godot;

public class Pylon : GenericCarrier
{
    public override void _Ready()
    {
        if (Globals.LastBuilding != null && Globals.LastBuilding.GlobalPosition.DistanceTo(GlobalPosition) < Pipe.MaxLength * Globals.TileSize)
        {
            Globals.LastBuilding.AddOutput(this);
        }

        base._Ready();
    }
}
