using Godot;
using System;

public class HealthBar : PanelContainer
{
    public float Health
    {
        get => (float)ProgressBar.Value / 100;
        set
        {
            ProgressBar.Value = value * 100;
        }
    }

    public ProgressBar ProgressBar;

    public override void _Ready()
    {
        base._Ready();

        ProgressBar = GetNode<ProgressBar>("ProgressBar");
    }
}
