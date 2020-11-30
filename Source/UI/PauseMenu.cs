using Godot;

public class PauseMenu : PanelContainer
{
    [Signal] public delegate void Quit();

    public Control Main;

    public Label MasterVolumeLabel;
    public Slider MasterVolumeSlider;

    public Label SFXVolumeLabel;
    public Slider SFXVolumeSlider;

    public Label MusicVolumeLabel;
    public Slider MusicVolumeSlider;

    public Control Confirmation;

    public override void _Ready()
    {
        base._Ready();

        Main = GetNode<Control>("Main");

        MasterVolumeLabel = GetNode<Label>("Main/MasterVolume/Label");
        MasterVolumeSlider = GetNode<Slider>("Main/MasterVolume/Slider");

        SFXVolumeLabel = GetNode<Label>("Main/SFXVolume/Label");
        SFXVolumeSlider = GetNode<Slider>("Main/SFXVolume/Slider");

        MusicVolumeLabel = GetNode<Label>("Main/MusicVolume/Label");
        MusicVolumeSlider = GetNode<Slider>("Main/MusicVolume/Slider");

        Confirmation = GetNode<Control>("Confirmation");

        UpdateMasterVolume(Settings.MasterVolume * 100);
        UpdateSFXVolume(Settings.SFXVolume * 100);
        UpdateMusicVolume(Settings.MusicVolume * 100);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (Input.IsActionJustPressed("pause"))
        {
            Main.Visible = true;
            Confirmation.Visible = false;

            if (Visible)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void _OnExitButtonUp()
    {
        Main.Visible = false;
        Confirmation.Visible = true;
    }

    public void _OnExitConfirmed()
    {
        GetTree().Paused = false;
        EmitSignal("Quit");
    }

    public void _OnExitCancelled()
    {
        Main.Visible = true;
        Confirmation.Visible = false;
    }

    public void Pause()
    {
        GetTree().Paused = true;
        Visible = true;
    }

    public void Resume()
    {
        GetTree().Paused = false;
        Visible = false;
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
