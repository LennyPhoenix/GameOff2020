using Godot;
using Godot.Collections;

public class Drill : Building
{
    [Export] public int Size = 3;
    [Export] public float MiningAmount = 1;

    public AnimationPlayer SpriteAnimationPlayer;
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

        SpriteAnimationPlayer = GetNode<AnimationPlayer>("Sprite/AnimationPlayer");
        OreTiles = GetTree().CurrentScene.GetNode<TileMap>("Planet/Ore");

        Ores = GetOres();

        if (Ores.Count == 0)
        {
            WarningSprite.Visible = true;
            MiningContainer.Visible = false;
        }

        int columns = Mathf.Min(4, Ores.Count);
        int rows = Mathf.CeilToInt((float)Ores.Count / 4);
        MiningContainer.RectSize = new Vector2(columns * 16 + 2 + Mathf.Min(columns - 1, 0), rows * 16 + 2 + Mathf.Min(rows - 1, 0));
        MiningGridContainer.Columns = columns;

        foreach (Ore ore in Ores.Keys)
        {
            minedCounters.Add(ore, 0f);

            var miningItem = (StorageItem)StorageItemScene.Instance();
            MiningGridContainer.AddChild(miningItem);
            miningItem.ItemType = (Item)ore;
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
            var item = (Item)ore;

            minedCounters[ore] += MiningAmount * Ores[ore];

            int count = Mathf.FloorToInt(minedCounters[ore]);
            minedCounters[ore] %= 1;

            Items[item] += count;
            Items[item] = Mathf.Min(Items[item], MaxStorage[item]);
        }
    }

    private Dictionary<Ore, int> GetOres()
    {
        Vector2 offset = new Vector2(Mathf.FloorToInt(Size / 2), Mathf.FloorToInt(Size / 2));
        Vector2 topLeft = (GlobalPosition / 16) - offset;

        var ores = new Dictionary<Ore, int>();

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                int oreId = OreTiles.GetCell(x + Mathf.FloorToInt(topLeft.x), y + Mathf.FloorToInt(topLeft.y));
                var ore = (Ore)oreId;

                if (oreId != -1 && MaxStorage.ContainsKey((Item)ore))
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
        }

        return ores;
    }
}
