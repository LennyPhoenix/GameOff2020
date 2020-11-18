using Godot;
using Godot.Collections;

public class Drill : Building
{
    [Export] public int Size = 3;
    [Export] public int MiningAmount = 1;

    public TileMap OreTiles;
    public Array<Ore> Ores;

    public override void _Ready()
    {
        base._Ready();

        OreTiles = GetTree().CurrentScene.GetNode<TileMap>("Planet/Ore");
        Ores = getOres();
    }

    public override void Tick()
    {
        if (Deleting)
        {
            return;
        }

        base.Tick();

        foreach (Ore ore in Ores)
        {
            var item = (Item)ore;

            if (MaxStorage.ContainsKey(item))
            {
                Items[item] += MiningAmount;
                Items[item] = Mathf.Min(Items[item], MaxStorage[item]);
            }
        }
    }

    private Array<Ore> getOres()
    {
        Vector2 offset = new Vector2(Mathf.FloorToInt(Size / 2), Mathf.FloorToInt(Size / 2));
        Vector2 topLeft = (GlobalPosition / 16) - offset;

        var ores = new Array<Ore>();

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                int ore = OreTiles.GetCell(x + Mathf.FloorToInt(topLeft.x), y + Mathf.FloorToInt(topLeft.y));
                if (ore != -1)
                {
                    ores.Add((Ore)ore);
                }
            }
        }

        return ores;
    }
}
