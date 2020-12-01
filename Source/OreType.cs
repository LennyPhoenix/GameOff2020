using Godot;

public class OreType : Label
{
    public TileMap OreTiles;

    public override void _Ready()
    {
        base._Ready();

        OreTiles = GetTree().CurrentScene.GetNode<TileMap>("Planet/Ore");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        RectGlobalPosition = GetGlobalMousePosition() + new Vector2(10, 10);

        var orePos = new Vector2()
        { 
            x = Mathf.FloorToInt(GetGlobalMousePosition().x / Globals.TileSize),
            y = Mathf.FloorToInt(GetGlobalMousePosition().y / Globals.TileSize)
        };

        int oreId = OreTiles.GetCellv(orePos);
        if (oreId != -1)
        {
            Ore ore = (Ore)oreId;

            Text = ore.ToString();
        }
        else
        {
            Text = "";
        }
    }
}
