using Godot;
using Godot.Collections;

public class Blueprint : Resource
{
    [Export] public Texture BuildTexture;
    [Export] public PackedScene Scene;
    [Export] public Vector2 Size = new Vector2(1, 1);

    [Export] public Dictionary<Item, int> Cost = new Dictionary<Item, int>();

    [Export(PropertyHint.MultilineText)] public string Description = "";
}
