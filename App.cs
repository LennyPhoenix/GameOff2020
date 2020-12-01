using Godot;
using Godot.Collections;

public class App : Node
{
    [Export] public PackedScene PlanetScene;

    public DialogueBox IntroDialogue;
    public AnimationPlayer IntroAnimationPlayer;

    public Control MainMenu;
    public AnimationPlayer MainMenuAnimationPlayer;
    public Button StartButton;

    public Label MasterVolumeLabel;
    public Slider MasterVolumeSlider;

    public Label SFXVolumeLabel;
    public Slider SFXVolumeSlider;

    public Label MusicVolumeLabel;
    public Slider MusicVolumeSlider;

    public AnimationPlayer WinAnimationPlayer;

    public Planet Planet;

    public override void _Ready()
    {
        base._Ready();

        IntroDialogue = GetNode<DialogueBox>("UI/IntroDialogue");
        IntroAnimationPlayer = GetNode<AnimationPlayer>("UI/IntroDialogue/AnimationPlayer");

        MainMenu = GetNode<Control>("UI/MainMenu");
        MainMenuAnimationPlayer = GetNode<AnimationPlayer>("UI/MainMenu/AnimationPlayer");
        StartButton = GetNode<Button>("UI/MainMenu/StartButton");

        MasterVolumeLabel = GetNode<Label>("UI/MainMenu/Volume/VBoxContainer/MasterVolume/Label");
        MasterVolumeSlider = GetNode<Slider>("UI/MainMenu/Volume/VBoxContainer/MasterVolume/Slider");

        SFXVolumeLabel = GetNode<Label>("UI/MainMenu/Volume/VBoxContainer/SFXVolume/Label");
        SFXVolumeSlider = GetNode<Slider>("UI/MainMenu/Volume/VBoxContainer/SFXVolume/Slider");

        MusicVolumeLabel = GetNode<Label>("UI/MainMenu/Volume/VBoxContainer/MusicVolume/Label");
        MusicVolumeSlider = GetNode<Slider>("UI/MainMenu/Volume/VBoxContainer/MusicVolume/Slider");

        WinAnimationPlayer = GetNode<AnimationPlayer>("UI/WinScreen/AnimationPlayer");

        UpdateMasterVolume(Settings.MasterVolume * 100);
        UpdateSFXVolume(Settings.SFXVolume * 100);
        UpdateMusicVolume(Settings.MusicVolume * 100);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (IntroDialogue.PageIndex == 1)
        {
            IntroDialogue.CharTime = 0.05f;
        }

        if (IntroDialogue.PageIndex == 2 && IntroAnimationPlayer.AssignedAnimation != "Show")
        {
            IntroAnimationPlayer.Play("Show");
        }

        Array sfx2D = GetTree().GetNodesInGroup("SFXAudio2D");

        foreach (AudioStreamPlayer2D audioStream in sfx2D)
        {
            audioStream.VolumeDb = -20 + (20 * Settings.MasterVolume * Settings.SFXVolume);
        }

        Array sfx = GetTree().GetNodesInGroup("SFXAudio");

        foreach (AudioStreamPlayer audioStream in sfx)
        {
            audioStream.VolumeDb = -20 + (20 * Settings.MasterVolume * Settings.SFXVolume);
        }

        Array music = GetTree().GetNodesInGroup("MusicAudio");

        foreach (AudioStreamPlayer audioStream in music)
        {
            audioStream.VolumeDb = -20 + (20 * Settings.MasterVolume * Settings.MusicVolume);
        }
    }

    public void _OnWin()
    {
        Planet = null;
        WinAnimationPlayer.Play("Show");
    }

    public void _OnQuit()
    {
        Planet = null;

        UpdateMasterVolume(Settings.MasterVolume * 100);
        UpdateSFXVolume(Settings.SFXVolume * 100);
        UpdateMusicVolume(Settings.MusicVolume * 100);
        MainMenuAnimationPlayer.Play("Show");
    }

    public void StartPlanet()
    {
        AddChild(Planet);

        Planet.Connect("Quit", this, "_OnQuit");
        Planet.Connect("Win", this, "_OnWin");
    }

    public async void _OnIntroDone()
    {
        if (Planet == null)
        {
            IntroAnimationPlayer.Play("Hide");

            Planet = (Planet)PlanetScene.Instance();

            await ToSignal(GetTree().CreateTimer(1.2f), "timeout");

            StartPlanet();
        }
    }

    public async  void _OnStartButtonUp()
    {
        MainMenuAnimationPlayer.Play("Hide");

        await ToSignal(GetTree().CreateTimer(1.2f), "timeout");

        IntroDialogue.Start();
        IntroDialogue.Visible = true;
    }

    public void _OnQuitButtonUp()
    {
        GetTree().Quit();
    }

    public void _OnFullscreenButtonUp()
    {
        OS.WindowFullscreen = !OS.WindowFullscreen;
    }

    public void UpdateMasterVolume(float value)
    {
        Settings.MasterVolume = value / 100;
        if (value != MasterVolumeSlider.Value)
        {
            MasterVolumeSlider.Value = value;
        }
        MasterVolumeLabel.Text = "Master Volume: " + value.ToString() + "%";
    }

    public void UpdateSFXVolume(float value)
    {
        Settings.SFXVolume = value / 100;
        if (value != SFXVolumeSlider.Value)
        {
            SFXVolumeSlider.Value = value;
        }
        SFXVolumeLabel.Text = "SFX Volume: " + value.ToString() + "%";
    }

    public void UpdateMusicVolume(float value)
    {
        Settings.MusicVolume = value / 100;
        if (value != MusicVolumeSlider.Value)
        {
            MusicVolumeSlider.Value = value;
        }
        MusicVolumeLabel.Text = "Music Volume: " + value.ToString() + "%";
    }
}
