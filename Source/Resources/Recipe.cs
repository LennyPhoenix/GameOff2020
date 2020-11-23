using Godot;
using Godot.Collections;

public class Recipe : Resource
{
    [Export] public Dictionary<Item, int> Costs = new Dictionary<Item, int>();
    [Export] public Dictionary<Item, int> Produces = new Dictionary<Item, int>();
}
