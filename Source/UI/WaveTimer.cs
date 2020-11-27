using Godot;
using System;

public class WaveTimer : PanelContainer
{
    [Signal] public delegate void TimerFinished();

    private int secondsRemaining = 0;
    [Export] public int SecondsRemaining
    {
        get => secondsRemaining;
        set
        {
            secondsRemaining = value;

            UpdateValues();
        }
    }

    private int wave = 0;
    [Export] public int Wave
    {
        get => wave;
        set
        {
            wave = value;

            UpdateValues();
        }
    }

    private int enemiesRemaining = 0;
    [Export] public int EnemiesRemaining
    {
        get => enemiesRemaining;
        set
        {
            enemiesRemaining = value;

            UpdateValues();
        }
    }

    public Timer Timer;
    public AnimationPlayer AnimationPlayer;
    public Label WaveLabel;
    public Label TimerLabel;
    public Label EnemiesLabel;

    public override void _Ready()
    {
        base._Ready();

        Timer = GetNode<Timer>("Timer");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        WaveLabel = GetNode<Label>("HBoxContainer/Wave");
        TimerLabel = GetNode<Label>("HBoxContainer/Timer");
        EnemiesLabel = GetNode<Label>("HBoxContainer/Enemies");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        SecondsRemaining = Mathf.FloorToInt(Timer.TimeLeft);
    }

    public void _OnTimeout()
    {
        EmitSignal("TimerFinished");
    }

    public void UpdateValues()
    {
        WaveLabel.Text = "Wave " + Wave.ToString() + ":";

        EnemiesLabel.Text = EnemiesRemaining.ToString() + " Enemies";

        TimeSpan timeSpan = TimeSpan.FromSeconds(secondsRemaining);
        TimerLabel.Text = timeSpan.ToString(@"mm\:ss");

        if (EnemiesRemaining != 0 && AnimationPlayer.AssignedAnimation == "Counting")
        {
            AnimationPlayer.Play("WaveArrived");
        }
        else if (EnemiesRemaining == 0 && AnimationPlayer.AssignedAnimation == "WaveArrived")
        {
            AnimationPlayer.Play("Counting");
        }
    }

    public void StartTimer(int seconds)
    {
        Timer.Start(seconds);
        SecondsRemaining = seconds;
    }
}
