using Godot;

public class FailureBox : PanelContainer
{
    [Signal] public delegate void Quit();

    public int Score
    {
        set
        {
            RichTextLabel.BbcodeText = "[center]The core contained [color=#b040c0]" + value + "[/color]/25 Blastium Fuel Cells.[/center]";
        }
    }

    public RichTextLabel RichTextLabel;

    public override void _Ready()
    {
        base._Ready();

        RichTextLabel = GetNode<RichTextLabel>("VBoxContainer/RichTextLabel");

        Score = 0;
    }

    public void _OnMainMenuButtonUp()
    {
        GetTree().Paused = false;
        EmitSignal("Quit");
    }
}
