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

    private Array<MinimapIcon> iconNodes = new Array<MinimapIcon>();
    private Array<Sprite> markers = new Array<Sprite>();

    public override void _Ready()
    {
        base._Ready();

        Markers = GetNode<Control>("MarginContainer/Markers");

        PlayerMarker = GetNode<Sprite>("MarginContainer/PlayerMarker");

        Tiles = GetNode<Sprite>("MarginContainer/Markers/Tiles");

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

        Array mapObjects = GetTree().GetNodesInGroup("MinimapIcons");
        foreach (MinimapIcon minimapIcon in mapObjects)
        {
            Sprite newMarker = (Sprite)icons[minimapIcon.IconType].Duplicate();
            Markers.AddChild(newMarker);
            newMarker.Show();
            iconNodes.Add(minimapIcon);
            markers.Add(newMarker);
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (PlayerPath == null || RootPlanetPath == null)
        {
            return;
        }

        var player = GetNode<Player>(PlayerPath);
        var planet = GetNode<Planet>(RootPlanetPath);

        PlayerMarker.Rotation = player.RotateGroup.GlobalRotation;
        Tiles.Position = (-player.GlobalPosition) / Globals.TileSize + Markers.RectSize / 2 + new Vector2(planet.WorldSize / 2, planet.WorldSize / 2);

        Array globalIconNodes = GetTree().GetNodesInGroup("MinimapIcons");

        foreach (MinimapIcon iconNode in globalIconNodes)
        {
            if (!iconNodes.Contains(iconNode))
            {
                Sprite newMarker = (Sprite)icons[iconNode.IconType].Duplicate();
                Markers.AddChild(newMarker);
                newMarker.Show();
                iconNodes.Add(iconNode);
                markers.Add(newMarker);
            }
        }

        Array<Sprite> removeMarkers = new Array<Sprite>();

        for (int i = 0; i < iconNodes.Count; i++)
        {
            MinimapIcon iconNode = iconNodes[i];
            Sprite marker = markers[i];

            if (globalIconNodes.Contains(iconNode))
            {
                Vector2 objPosition = (iconNode.GetNode<Node2D>(iconNode.Root).GlobalPosition - player.GlobalPosition) / Globals.TileSize + Markers.RectSize / 2;

                if (iconNode.HideOutsideMap)
                {
                    marker.Visible = Markers.GetRect().HasPoint(objPosition);
                }

                objPosition.x = Mathf.Clamp(objPosition.x, 0, Markers.RectSize.x);
                objPosition.y = Mathf.Clamp(objPosition.y, 0, Markers.RectSize.y);

                marker.Position = objPosition;
            }
            else
            {
                marker.QueueFree();
                removeMarkers.Add(marker);
            }
        }

        foreach (Sprite marker in removeMarkers)
        {
            int i = markers.IndexOf(marker);
            markers.RemoveAt(i);
            iconNodes.RemoveAt(i);
        }
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

        var planet = GetNode<Planet>(RootPlanetPath);
        var ore = GetNode<TileMap>(OrePath);
        var wall = GetNode<TileMap>(WallPath);

        Image image = new Image();
        image.Create(planet.WorldSize + planet.PerimeterSize * 2, planet.WorldSize + planet.PerimeterSize * 2, false, Image.Format.Rgba8);
        image.Lock();

        for (int x = 0 - planet.PerimeterSize; x < (planet.WorldSize + planet.PerimeterSize); x++)
        {
            for (int y = 0 - planet.PerimeterSize; y < (planet.WorldSize + planet.PerimeterSize); y++)
            {
                Color color;
                if (wall.GetCell(x, y) == -1)
                {
                    switch (ore.GetCell(x, y))
                    {
                        case 0: // Stone
                            color = new Color("626262");
                            break;
                        case 1: // Copper
                            color = new Color("c67a31");
                            break;
                        case 2: // Iron
                            color = new Color("423444");
                            break;
                        case 3: // Titanium
                            color = new Color("7084c4");
                            break;
                        default:
                            color = new Color("cccccc");
                            break;
                    }
                }
                else
                {
                    color = new Color("101010");
                }

                image.SetPixel(x + planet.PerimeterSize, y + planet.PerimeterSize, color);
            }
        }
        image.Unlock();

        ImageTexture imageTexture = new ImageTexture();
        imageTexture.CreateFromImage(image, 0);

        Tiles.Texture = imageTexture;
    }
}
