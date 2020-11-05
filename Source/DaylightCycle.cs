using Godot;
using System;

[Tool]
public class DaylightCycle : CanvasModulate
{
    private Color dayColor = new Color("fcf7d7");
    [Export] public Color DayColor
    {
        get => dayColor;
        set
        {
            dayColor = value;
            if (Engine.EditorHint && Forward)
            {
                Color = value;
            }
        }
    }

    private Color nightColor = new Color("232544");
    [Export] public Color NightColor
    {
        get => nightColor;
        set
        {
            nightColor = value;
            if (Engine.EditorHint && !Forward)
            {
                Color = value;
            }
        }
    }
    [Export] public float CycleLength = 100f;

    private bool forward = true;
    [Export] public bool Forward
    {
        get => forward;
        set
        {
            forward = value;
            if (Engine.EditorHint)
            {
                Color = value ? DayColor : NightColor;
            }
        }
    }

    public Tween Tween;

    public override void _Ready()
    {
        base._Ready();

        Color = Forward ? DayColor : NightColor;

        if (Engine.EditorHint)
        {
            return;
        }

        Tween = GetNode<Tween>("Tween");

        StartTween();
    }

    public void _OnTweenCompleted()
    {
        Forward = !Forward;
        StartTween();
    }

    public void StartTween()
    {
        Color start = Forward ? DayColor : NightColor;
        Color end = !Forward ? DayColor : NightColor;

        Tween.InterpolateProperty(
            this, "color",
            start, end,
            CycleLength,
            Tween.TransitionType.Quart,
            Tween.EaseType.Out
        );
        Tween.Start();
    }
}
