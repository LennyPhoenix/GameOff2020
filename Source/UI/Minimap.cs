using Godot;
using Godot.Collections;

public class Minimap : MarginContainer
{
    [Export] public NodePath RootPlanetPath;
    [Export] public NodePath PlayerPath;
    [Export] public NodePath WallPath;
    [Export] public NodePath OrePath;

    [Export] public float Zoom = 1.5f;

    public Control Markers;
    public Sprite Tiles;
    public Sprite PlayerMarker;
    public Sprite CoreMarker;
    public Sprite DrillMarker;
    public Sprite EnemyMarker;
    public Sprite TurretMarker;

    private Dictionary<MinimapIconType, Sprite> icons;

    public Player Player;
    public Planet Planet;

    public override void _Ready()
    {
        base._Ready();

        Markers = GetNode<Control>("MarginContainer/Markers");

        PlayerMarker = GetNode<Sprite>("MarginContainer/PlayerMarker");

        Tiles = GetNode<Sprite>("MarginContainer/TilesClipper/Tiles");

        CoreMarker = GetNode<Sprite>("MarginContainer/Markers/CoreMarker");
        DrillMarker = GetNode<Sprite>("MarginContainer/Markers/DrillMarker");
        EnemyMarker = GetNode<Sprite>("MarginContainer/Markers/EnemyMarker");
        TurretMarker = GetNode<Sprite>("MarginContainer/Markers/TurretMarker");

        icons = new Dictionary<MinimapIconType, Sprite>()
        {
            { MinimapIconType.Core, CoreMarker },
            { MinimapIconType.Drill, DrillMarker },
            { MinimapIconType.Enemy, EnemyMarker },
            { MinimapIconType.Turret, TurretMarker }
        };

        PlayerMarker.Position = Markers.RectSize / 2 + Markers.RectPosition;

        Globals.Minimap = this;
    }

    public Sprite AddMarker(MinimapIconType iconType)
    {
        Sprite newMarker = (Sprite)icons[iconType].Duplicate();
        Markers.AddChild(newMarker);
        newMarker.Show();
        return newMarker;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (PlayerPath == null || RootPlanetPath == null)
        {
            return;
        }

        Player = GetNode<Player>(PlayerPath);
        Planet = GetNode<Planet>(RootPlanetPath);

        PlayerMarker.Rotation = Player.RotateGroup.GlobalRotation;
        Tiles.Position = (-Player.GlobalPosition) / Globals.TileSize + Markers.RectSize / 2 + new Vector2(Planet.WorldSize / 2, Planet.WorldSize / 2);
    }

    public void _OnPlanetGenerated()
    {
        UpdateTiles();
    }

    public void UpdateTiles()
    {
        if (RootPlanetPath == null || OrePath == null || WallPath == null)
        {
            return;
        }

        Planet = GetNode<Planet>(RootPlanetPath);
        var ore = GetNode<TileMap>(OrePath);
        var wall = GetNode<TileMap>(WallPath);

        Image image = new Image();
        image.Create(Planet.WorldSize + Planet.PerimeterSize * 2, Planet.WorldSize + Planet.PerimeterSize * 2, false, Image.Format.Rgba8);
        image.Lock();

        for (int x = 0 - Planet.PerimeterSize; x < (Planet.WorldSize + Planet.PerimeterSize); x++)
        {
            for (int y = 0 - Planet.PerimeterSize; y < (Planet.WorldSize + Planet.PerimeterSize); y++)
            {
                Color color;
                if (wall.GetCell(x, y) == -1)
                {
                    switch (ore.GetCell(x, y))
                    {
                        case (int)Ore.Stone:
                            color = new Color("626262");
                            break;
                        case (int)Ore.Copper:
                            color = new Color("c67a31");
                            break;
                        case (int)Ore.Iron:
                            color = new Color("423444");
                            break;
                        case (int)Ore.Titanium:
                            color = new Color("7084c4");
                            break;
                        case (int)Ore.Coal:
                            color = new Color("202020");
                            break;
                        case (int)Ore.Blastium:
                            color = new Color("dd00f7");
                            break;
                        default:
                            color = new Color("cccccc");
                            break;
                    }
                }
                else
                {
                    color = new Color("000000");
                }

                image.SetPixel(x + Planet.PerimeterSize, y + Planet.PerimeterSize, color);
            }
        }
        image.Unlock();

        ImageTexture imageTexture = new ImageTexture();
        imageTexture.CreateFromImage(image, 0);

        Tiles.Texture = imageTexture;
    }
}
