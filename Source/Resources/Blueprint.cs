using Godot;
using Godot.Collections;

public class Blueprint : Resource
{
    [Export] public Texture BuildTexture;
    [Export] public PackedScene Scene;
    [Export] public Vector2 Size = new Vector2(1, 1);

    [Export]
    public Dictionary<Ore, int> Cost = new Dictionary<Ore, int>()
    {
        { Ore.Stone, 0 },
        { Ore.Copper, 0 },
        { Ore.Iron, 0 },
        { Ore.Titanium, 0 },
    };
}
