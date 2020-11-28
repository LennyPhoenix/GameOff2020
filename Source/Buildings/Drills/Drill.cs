using Godot;
using Godot.Collections;

public class Drill : Building
{
    [Export] public float MiningAmount = 1;

    public TileMap OreTiles;
    public Dictionary<Ore, int> Ores;

    public PanelContainer MiningContainer;
    public GridContainer MiningGridContainer;
    public Dictionary<Ore, StorageItem> MiningItems = new Dictionary<Ore, StorageItem>();

    private Dictionary<Ore, float> minedCounters = new Dictionary<Ore, float>();

    public override void _Ready()
    {
        MiningContainer = GetNode<PanelContainer>("UI/MiningContainer");
        MiningGridContainer = GetNode<GridContainer>("UI/MiningContainer/GridContainer");

        base._Ready();

        OreTiles = GetTree().CurrentScene.GetNode<TileMap>("Planet/Ore");

        Ores = GetOres();

        int coverage = 0;
        foreach (Ore ore in Ores.Keys)
        {
            coverage += Ores[ore];
        }
        SpriteAnimationPlayer.PlaybackSpeed = coverage / Mathf.Pow(Size, 2);

        if (Ores.Count == 0)
        {
            WarningSprite.Visible = true;
            MiningContainer.Visible = false;
        }

        int columns = Mathf.Clamp(Ores.Count, 1, 4);
        int rows = Mathf.CeilToInt((float)Ores.Count / 4);
        MiningContainer.RectSize = new Vector2(columns * 16 + 2 + Mathf.Min(columns - 1, 0), rows * 16 + 2 + Mathf.Min(rows - 1, 0));
        MiningGridContainer.Columns = columns;

        foreach (Ore ore in Ores.Keys)
        {
            minedCounters.Add(ore, 0f);

            var miningItem = (StorageItem)StorageItemScene.Instance();
            MiningGridContainer.AddChild(miningItem);
            miningItem.ItemType = Globals.OreToItem[ore];
            miningItem.CountFloat = Ores[ore] * MiningAmount;

            MiningItems.Add(ore, miningItem);
        }

        Globals.LastBuilding = this;
    }

    public override void Tick()
    {
        if (Deleting)
        {
            return;
        }

        base.Tick();

        foreach (Ore ore in Ores.Keys)
        {
            minedCounters[ore] += MiningAmount * Ores[ore];

            int count = Mathf.FloorToInt(minedCounters[ore]);
            minedCounters[ore] %= 1;

            var item = Globals.OreToItem[ore];

            Items[item] += count;
            Items[item] = Mathf.Min(Items[item], MaxStorage[item]);
        }
    }

    private Dictionary<Ore, int> GetOres()
    {
        var ores = new Dictionary<Ore, int>();

        Array<Vector2> positions = GetCoveredTiles();
        foreach (Vector2 pos in positions)
        {
            int oreId = OreTiles.GetCell(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
            var ore = (Ore)oreId;

            if (oreId != -1 && MaxStorage.ContainsKey(Globals.OreToItem[ore]))
            {
                if (!ores.ContainsKey(ore))
                {
                    ores.Add(ore, 1);
                }
                else
                {
                    ores[ore] += 1;
                }
            }
        }

        return ores;
    }
}
