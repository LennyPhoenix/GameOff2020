using Godot;
using Godot.Collections;
using System;
using System.Security.Cryptography.X509Certificates;

[Tool]
public class DaylightCycle : CanvasModulate
{
    public Color dayColor = new Color("fcf7d7");
    [Export]
    public Color DayColor
    {
        get { return dayColor; }
        set { 
            dayColor = value;
            if (Engine.EditorHint && forward)
            {
                Color = value;
            }
        }
    }

    public Color nightColor = new Color("232544");
    [Export]
    public Color NightColor
    {
        get { return nightColor; }
        set
        {
            nightColor = value;
            if (Engine.EditorHint && !forward)
            {
                Color = value;
            }
        }
    }
    [Export]
    public float cycleLength = 100f;

    public bool forward = true;
    [Export]
    public bool Forward
    {
        get { return forward;  }
        set
        {
            forward = value;
            if (Engine.EditorHint)
            {
                Color = forward ? dayColor : nightColor;
            }
        }
    }

    public Tween tween;

    public override void _Ready()
    {
        base._Ready();

        Color = forward ? dayColor : nightColor;

        if (Engine.EditorHint)
        {
            return;
        }

        tween = GetNode<Tween>("Tween");

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
            cycleLength,
            Tween.TransitionType.Quart,
            Tween.EaseType.Out
        );
        tween.Start();
    }
}
