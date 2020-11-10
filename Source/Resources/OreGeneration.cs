using Godot;
using System;


public class OreGeneration : OpenSimplexNoise
{
	[Export] public Ore OreId = Ore.Stone;
	[Export] public float Threshold = 0.65f;
}
