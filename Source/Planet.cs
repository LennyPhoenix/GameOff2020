using Godot;
using System;

[Tool]
public class Planet : Node2D
{
	[Signal] public delegate void Generated();

	[Export] public OpenSimplexNoise GroundNoise;
	[Export] public OpenSimplexNoise WallNoise;

	[Export] public int WorldSize = 300;
	[Export] public int PerimeterSize = 32;

	[Export] public int GroundTileRange = 2;
	[Export] public int WallTileRange = 2;
	[Export] public float WallTileThreshold = 0.65f;

	public int RealWorldSize {
		get { return WorldSize * (int)GroundTiles.CellSize.x; }
	}

	public Image GroundNoiseImage;
	public Image WallNoiseImage;

	public TileMap GroundTiles;
	public TileMap WallTiles;
	public Player Player;

	public override void _Ready()
	{
		base._Ready();

		if (Engine.EditorHint)
		{
			return;
		}

		GroundTiles = GetNode<TileMap>("Ground");
		WallTiles = GetNode<TileMap>("Walls");

		Player = GetNode<Player>("GroundEntities/Player");
		Player.Position = WorldSize * GroundTiles.CellSize / 2;

		Generate();
	}

	public override string _GetConfigurationWarning()
	{
		if (GroundNoise is null)
		{
			return "GroundNoise property is empty.";
		}
		else if (WallNoise is null)
		{
			return "WallNoise property is empty.";
		}
		return "";
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);

		if (Engine.EditorHint)
        {
			return;
        }

		GetTree().CallGroup(
			"MapWrapper", "Wrap",
			RealWorldSize * Mathf.CeilToInt((float)PerimeterSize / WorldSize),
			RealWorldSize
		);
	}

	public void Generate()
	{
		GroundNoiseImage = GroundNoise.GetSeamlessImage(WorldSize);
		GroundNoiseImage.Lock();

		WallNoiseImage = WallNoise.GetSeamlessImage(WorldSize);
		WallNoiseImage.Lock();

		GenerateWorld();
		GeneratePerimeter();

		EmitSignal("Generated");
	}

	private void GenerateWorld()
	{
		for (int x = 0; x < WorldSize; x++)
		{
			for (int y = 0; y < WorldSize; y++)
			{
				float wallProb = WallNoiseImage.GetPixel(x, y).v;
				if (wallProb > WallTileThreshold)
				{
					float noiseValue = (WallNoiseImage.GetPixel(x, y).v - WallTileThreshold) / (1f - WallTileThreshold);
					int tileId = Mathf.FloorToInt(noiseValue * WallTileRange);
					WallTiles.SetCell(x, y, tileId);
				}
				else
				{
					int tileId = Mathf.FloorToInt(GroundNoiseImage.GetPixel(x, y).v * GroundTileRange);
					GroundTiles.SetCell(x, y, tileId);
				}
			}
		}
	}

	private void GeneratePerimeter()
	{
		for (int x = 0 - PerimeterSize; x < (WorldSize + PerimeterSize); x++)
		{
			for (int y = 0 - PerimeterSize; y < (WorldSize + PerimeterSize); y++)
			{
				if (
					x < 0 || x >= WorldSize
					|| y < 0 || y >= WorldSize
				)
				{
					GroundTiles.SetCell(x, y, GroundTiles.GetCell(
						(x + WorldSize * Mathf.CeilToInt((float)PerimeterSize / WorldSize)) % WorldSize,
						(y + WorldSize * Mathf.CeilToInt((float)PerimeterSize / WorldSize)) % WorldSize
					));
					WallTiles.SetCell(x, y, WallTiles.GetCell(
						(x + WorldSize) % WorldSize,
						(y + WorldSize) % WorldSize
					));
				}
			}
		}
	}
}
