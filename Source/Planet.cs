using Godot;
using Godot.Collections;

[Tool]
public class Planet : Navigation2D
{
	[Signal] public delegate void Generated();

	[Export] public PackedScene EnemySmall;
	[Export] public PackedScene EnemyMedium;
	[Export] public PackedScene EnemyLarge;
	[Export] public PackedScene EnemyBoss;

	[Export] public OpenSimplexNoise GroundNoise;
	[Export] public OpenSimplexNoise WallNoise;

	[Export] public int WorldSize = 300;
	[Export] public int PerimeterSize = 96;

	[Export] public float ClearRadius = 16f;

	[Export] public int GroundTileRange = 2;
	[Export] public Array<float> WallTileThresholds = new Array<float>()
	{
		0.625f,
		0.675f
	};

	[Export] public Array<OreGeneration> OreGenerators = new Array<OreGeneration>();

	public int RealWorldSize {
		get { return WorldSize * (int)GroundTiles.CellSize.x; }
	}

	public Image GroundNoiseImage;
	public Image WallNoiseImage;
	public Array<Image> OreGenerationImages = new Array<Image>();

	public Area2D SpawnRadius;

	public TileMap GroundTiles;
	public TileMap OreTiles;
	public TileMap WallTiles;

	public Node2D Buildings;
	public Node2D GroundEntities;
	public Node2D Pipes;

	public Building Core;
	public Player Player;

	public WaveTimer WaveTimer;

	public override void _Ready()
	{
		base._Ready();

		if (Engine.EditorHint)
		{
			return;
		}

		SpawnRadius = GetNode<Area2D>("SpawnRadius");

		GroundTiles = GetNode<TileMap>("Ground");
		OreTiles = GetNode<TileMap>("Ore");
		WallTiles = GetNode<TileMap>("Walls");

		Buildings = GetNode<Node2D>("Buildings");
		GroundEntities = GetNode<Node2D>("GroundEntities");
		Pipes = GetNode<Node2D>("Pipes");

		Core = Buildings.GetNode<Building>("Core");
		Player = GroundEntities.GetNode<Player>("Player");

        WaveTimer = GetNode<WaveTimer>("UI/WaveTimer");

        Generate();

		WaveTimer.Wave = 1;
        WaveTimer.StartTimer(4); // 150
    }

	public override string _GetConfigurationWarning()
	{
		if (GroundNoise == null)
		{
			return "GroundNoise property is empty.";
		}
		else if (WallNoise == null)
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
			"MapWrapper", "Update",
			RealWorldSize * Mathf.CeilToInt((float)PerimeterSize / WorldSize),
			RealWorldSize,
			Player.Camera.GlobalPosition
		);
	}

	public void _OnBuildingClockTimeout()
	{
		GetTree().CallGroup("Buildings", "Tick");
	}

	public void _OnWaveTimerFinished()
    {
        int totalEnemies = 0;
        int num = GetTree().GetNodesInGroup("EnemySpawnTargets").Count;

        while (num > 0)
        {
            SpawnRadius.GlobalPosition = new Vector2()
            {
                x = (float)GD.RandRange(0, WorldSize) * Globals.TileSize,
                y = (float)GD.RandRange(0, WorldSize) * Globals.TileSize
            };

            if (SpawnRadius.GetOverlappingBodies().Count == 0)
            {
                num--;

                for (int i = 0; i < Mathf.FloorToInt(WaveTimer.Wave / 2) + 1; i++)
                {
                    var enemy = (Enemy)EnemySmall.Instance();
                    enemy.GlobalPosition = SpawnRadius.GlobalPosition + new Vector2(0, 64).Rotated(Mathf.Deg2Rad((float)GD.RandRange(0, 360)));

                    GroundEntities.AddChild(enemy);
					totalEnemies++;
                }
            }
			else
            {
				Array overlapping = SpawnRadius.GetOverlappingBodies();
				GD.Print(overlapping);
            }
        }

        WaveTimer.EnemiesRemaining = totalEnemies;
    }

	public void Generate()
	{
		GD.Randomize();

		GroundTiles.Clear();
		GroundNoise.Seed = (int)GD.Randi();
		GroundNoiseImage = GroundNoise.GetSeamlessImage(WorldSize);
		GroundNoiseImage.Lock();

		WallTiles.Clear();
		WallNoise.Seed = (int)GD.Randi();
		WallNoiseImage = WallNoise.GetSeamlessImage(WorldSize);
		WallNoiseImage.Lock();

		OreGenerationImages.Clear();
		OreTiles.Clear();
		foreach (OreGeneration oreGeneration in OreGenerators)
		{
			oreGeneration.Seed = (int)GD.Randi();
			Image image = oreGeneration.GetSeamlessImage(WorldSize);
			image.Lock();
			OreGenerationImages.Add(image);
		}

		GenerateWorld();
		GenerateOres();
		GeneratePerimeter();

		var regionStart = new Vector2(-PerimeterSize, -PerimeterSize);
		var regionEnd = new Vector2(WorldSize + PerimeterSize, WorldSize + PerimeterSize);

		GroundTiles.UpdateBitmaskRegion(regionStart, regionEnd);
		OreTiles.UpdateBitmaskRegion(regionStart, regionEnd);
		WallTiles.UpdateBitmaskRegion(regionStart, regionEnd);

		EmitSignal("Generated");

		int mid = WorldSize * Globals.TileSize / 2;

		Core.GlobalPosition = (new Vector2(mid, mid - 80) / 16).Round() * 16 + new Vector2(8, 8);
		Player.Position = new Vector2(mid, mid);
		Player.Camera.ForceUpdateScroll();
		Player.Camera.ResetSmoothing();
	}

	private void GenerateWorld()
	{
		int mid = WorldSize / 2;
		var midVec = new Vector2(mid, mid);
		for (int x = 0; x < WorldSize; x++)
		{
			for (int y = 0; y < WorldSize; y++)
			{
				float wallProb = WallNoiseImage.GetPixel(x, y).v;
				int wallId = -1;
				foreach (float threshold in WallTileThresholds)
				{
					if (wallProb > threshold)
					{
						wallId++;
						continue;
					}
					else
					{
						break;
					}
				}
				if (wallId > -1 && midVec.DistanceTo(new Vector2(x, y)) > ClearRadius)
				{
					WallTiles.SetCell(x, y, wallId);
				}
				else
				{
					int tileId = Mathf.FloorToInt(GroundNoiseImage.GetPixel(x, y).v * GroundTileRange);
					GroundTiles.SetCell(x, y, tileId);
				}
			}
		}
	}

	private void GenerateOres()
	{
		for (int x = 0; x < WorldSize; x++)
		{
			for (int y = 0; y < WorldSize; y++)
			{
				foreach (OreGeneration oreGeneration in OreGenerators)
				{
					Image image = OreGenerationImages[OreGenerators.IndexOf(oreGeneration)];
					float noiseValue = image.GetPixel(x, y).v;
					if (noiseValue > oreGeneration.Threshold)
					{
						OreTiles.SetCell(x, y, (int)oreGeneration.OreId);
					}
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
					OreTiles.SetCell(x, y, OreTiles.GetCell(
						(x + WorldSize * Mathf.CeilToInt((float)PerimeterSize / WorldSize)) % WorldSize,
						(y + WorldSize * Mathf.CeilToInt((float)PerimeterSize / WorldSize)) % WorldSize
					));
					WallTiles.SetCell(x, y, WallTiles.GetCell(
						(x + WorldSize * Mathf.CeilToInt((float)PerimeterSize / WorldSize)) % WorldSize,
						(y + WorldSize * Mathf.CeilToInt((float)PerimeterSize / WorldSize)) % WorldSize
					));
				}
			}
		}
	}
}
