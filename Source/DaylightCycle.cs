using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public class DaylightCycle : CanvasModulate
{
    [Export]
    public Color dayColor = new Color("f7eeb1");
    [Export]
    public Color nightColor = new Color("31345f");
    [Export]
    public float time = 100f;

    public bool forward = true;

    public Tween tween;

    public override void _Ready()
    {
        base._Ready();

        tween = GetNode<Tween>("Tween");

        Color = forward ? dayColor : nightColor;

        StartTween();
    }

    public void _OnTweenCompleted()
    {
        forward = !forward;
        StartTween();
    }

    public void StartTween()
    {
        Color start; 
        Color end;

        if (forward)
        {
            start = dayColor;
            end = nightColor;
        }
        else
        {
            start = nightColor;
            end = dayColor;
        }

        tween.InterpolateProperty(
            this, "color",
            start, end,
            time,
            Tween.TransitionType.Quart,
            Tween.EaseType.Out
        );
        tween.Start();
    }
}
