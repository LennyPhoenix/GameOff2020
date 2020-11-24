using Godot;

[Tool]
public class CircleRenderer : Node2D
{
    private float radius = 32f;
    [Export] public float Radius
    {
        get => radius;
        set
        {
            radius = value;
            Update();
        }
    }

    public override void _Draw()
    {
        DrawCircle(Position, Radius, new Color(1, 1, 1));
    }
}
