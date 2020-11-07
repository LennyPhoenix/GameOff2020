using Godot;
using System;

public class Planet : Node2D
{
    public enum Tile
    {
        Ground,
        Wall,
    }

    [Signal] public delegate void Generated();

    [Export] public Vector2 WorldSize = new Vector2(40, 24);
    [Export] public Vector2 PerimeterSize = new Vector2(18, 11);

    public Vector2 Size 
    {
        get { return WorldSize + 2 * PerimeterSize; }
    }

    public TileMap GroundTiles;
    public TileMap WallTiles;

    public override void _Ready()
    {
        base._Ready();

        GroundTiles = GetNode<TileMap>("Ground");
        WallTiles = GetNode<TileMap>("Walls");

        Generate();
    }

    public void Generate()
    {
        GeneratePerimeter(Tile.Wall, WallTiles);
        GenerateWorld(Tile.Ground, GroundTiles);

        EmitSignal("Generated");
    }

    private void GeneratePerimeter(Tile tile, TileMap tileMap)
    {
        for (int x = -(int)Size.x / 2; x < (int)Size.x / 2; x++)
        {
            for (int y = -(int)Size.y / 2; y < (int)Size.y / 2; y++)
            {
                if (
                    x < -(int)WorldSize.x / 2 || x >= (int)WorldSize.x / 2
                    || y < -(int)WorldSize.y / 2 || y >= (int)WorldSize.y / 2
                )
                { 
                    WallTiles.SetCell(x, y, (int)tile); 
                }
                
            }
        }
    }

    private void GenerateWorld(Tile groundTile, TileMap tileMap)
    {

        for (int x = -(int)WorldSize.x / 2 - 1; x < (int)WorldSize.x / 2 + 1; x++)
        {
            for (int y = -(int)WorldSize.y / 2 - 1; y < (int)WorldSize.y / 2 + 1; y++)
            {
                tileMap.SetCell(x, y, (int)groundTile);
            }
        }
    }
}
