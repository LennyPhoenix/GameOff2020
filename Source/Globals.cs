using Godot;
using Godot.Collections;

public enum MinimapIconType
{
	Turret,
	Drill,
	Core,
	Enemy,
}

public enum Ore
{
	Stone,
	Copper,
	Iron,
	Titanium,
	Coal,
	Blastium,
}

public enum Item
{
	Stone,
	Copper,
	Iron,
	Titanium,
	CopperBar,
	IronBar,
	TitaniumBar,
	Coal,
	Blastium,
	BlastiumCell,
}

public class Globals
{
	public const int TileSize = 16;

	public static BuildManager BuildManager;

	public static Building SelectedBuilding;
	public static Building DraggingBuilding;
	public static Building HoveringBuilding;
	public static Building LastBuilding;

	public static Core Core;

    public static Dictionary<Ore, Item> OreToItem = new Dictionary<Ore, Item>()
    {
		{ Ore.Stone, Item.Stone },
		{ Ore.Copper, Item.Copper },
		{ Ore.Iron, Item.Iron },
		{ Ore.Titanium, Item.Titanium },
		{ Ore.Coal, Item.Coal },
		{ Ore.Blastium, Item.Blastium },
	};

	public static void Reset()
    {
		BuildManager = null;

		SelectedBuilding = null;
		DraggingBuilding = null;
		HoveringBuilding = null;
		LastBuilding = null;

		Core = null;
	}
}