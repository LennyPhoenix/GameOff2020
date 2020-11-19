using Godot;

public class GenericCarrier : Building
{
    [Export] public int CanStore = 50;
    [Export] public int CanOutput = 50;

    public override void _Ready()
    {
        base._Ready();

        foreach (Item item in (Item[])System.Enum.GetValues(typeof(Item)))
        {
            MaxStorage.Add(item, CanStore);
            Outputs.Add(item, CanStore);
        }
    }
}
