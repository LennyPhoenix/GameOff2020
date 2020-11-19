using Godot;

public class Pylon : Building
{
    public override void _Ready()
    {
        if (Globals.LastBuilding != null)
        {
            Globals.LastBuilding.AddOutput(this);
        }

        base._Ready();

        foreach (Item item in (Item[])System.Enum.GetValues(typeof(Item)))
        {
            MaxStorage.Add(item, 50);
            Outputs.Add(item, 50);
        }
    }
}
