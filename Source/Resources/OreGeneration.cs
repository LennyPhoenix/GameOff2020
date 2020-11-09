using Godot;
using System;


public class OreGeneration : OpenSimplexNoise
{
	public enum Ore
	{
		Stone,
		Copper,
		Lead,
		Titanium,
	}

	[Export] public Ore OreId = Ore.Stone;
	[Export] public float Threshold = 0.65f;
}
