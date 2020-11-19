using Godot;

public enum Ore
{
	Stone,
	Copper,
	Iron,
	Titanium,
}

public enum MinimapIconType
{
	Turret,
	Drill,
	Core,
	Enemy
}

public enum Item
{
	Stone,
	Copper,
	Iron,
	Titanium,
}

public class Globals
{
	public const int TileSize = 16;

	public static bool BuildMode = false;

	public static Building SelectedBuilding;
	public static Building DraggingBuilding;
	public static Building HoveringBuilding;
	public static Building LastBuilding;
}