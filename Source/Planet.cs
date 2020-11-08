using Godot;
using System;

[Tool]
public class Planet : Node2D
{
    [Signal] public delegate void Generated();

    [Export] public OpenSimplexNoise GroundNoise;
    [Export] public OpenSimplexNoise WallNoise;

    [Export] public Vector2 WorldSize = new Vector2(300, 300);
    [Export] public Vector2 PerimeterSize = new Vector2(32, 32);

    [Export] public int PerimeterTileId = 0;
    [Export] public int GroundTileRange = 2;
    [Export] public float WallTileThreshold = 0.65f;
    [Export] public int WallTileRange = 2;

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

    public void Generate()
    {
        GeneratePerimeter();
        GenerateWorld();

        EmitSignal("Generated");
    }

    private void GeneratePerimeter()
    {
        for (int x = 0 - (int)PerimeterSize.x; x < ((int)WorldSize.x + (int)PerimeterSize.x); x++)
        {
            for (int y = 0 - (int)PerimeterSize.y; y < ((int)WorldSize.y + (int)PerimeterSize.y); y++)
            {
                if (
                    x < 0 || x >= (int)WorldSize.x
                    || y < 0 || y >= (int)WorldSize.y
                )
                {
                    WallTiles.SetCell(x, y, PerimeterTileId);
                }
            }
        }
    }

    private void GenerateWorld()
    {
        for (int x = 0; x < (int)WorldSize.x; x++)
        {
            for (int y = 0; y < (int)WorldSize.y; y++)
            {
                float wallProb = WallNoise.GetNoise2d(x, y) / 2 + .5f;
                if (wallProb > WallTileThreshold)
                {
                    float noiseValue = (WallNoise.GetNoise2d(x, y) / 2 + .5f - WallTileThreshold) / (1f - WallTileThreshold);
                    int tileId = Mathf.FloorToInt(noiseValue * WallTileRange);
                    WallTiles.SetCell(x, y, tileId);
                }
                else
                {
                    int tileId = Mathf.FloorToInt((GroundNoise.GetNoise2d(x, y) / 2 + .5f) * GroundTileRange);
                    GroundTiles.SetCell(x, y, tileId);
                }
            }
        }
    }
}
