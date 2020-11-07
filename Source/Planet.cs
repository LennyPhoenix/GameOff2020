using Godot;
using System;

[Tool]
public class Planet : Node2D
{
    [Signal] public delegate void Generated();

    [Export] public OpenSimplexNoise Noise;
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

        if (Engine.EditorHint)
        {
            return;
        }

        GroundTiles = GetNode<TileMap>("Ground");
        WallTiles = GetNode<TileMap>("Walls");

        Generate();
    }

    public override string _GetConfigurationWarning()
    {
        if (Noise is null)
        {
            return "Noise property is empty.";
        }
        return "";
    }

    public void Generate()
    {
        GeneratePerimeter(1, WallTiles);
        GenerateWorld(0, GroundTiles);

        EmitSignal("Generated");
    }

    private void GeneratePerimeter(int tileId, TileMap tileMap)
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
                    WallTiles.SetCell(x, y, tileId); 
                }
                
            }
        }
    }

    private void GenerateWorld(int groundTileId, TileMap tileMap)
    {

        for (int x = -(int)WorldSize.x / 2; x < (int)WorldSize.x / 2; x++)
        {
            for (int y = -(int)WorldSize.y / 2; y < (int)WorldSize.y / 2; y++)
            {
                tileMap.SetCell(x, y, groundTileId);
            }
        }
    }
}
