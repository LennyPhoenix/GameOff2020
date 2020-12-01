using Godot;
using Godot.Collections;

public class DialogueBox : HBoxContainer
{
    [Signal] public delegate void Done();

    [Export] public float CharTime = 0.025f;
    [Export] public float CommaTime = 0.1f;
    [Export] public float PeriodTime = 0.2f;

    private string text = "";
    [Export(PropertyHint.MultilineText)] public string Text
    {
        get => text;
        set
        {
            text = value;

            UpdatePages();
        }
    }
    public Array<string> Pages;

    public int PageIndex = 0;

    public AudioStreamPlayer Speak;

    public RichTextLabel TextLabel;

    public Button NextButton;
    public Button SkipButton;
    public Button ContinueButton;

    public override void _Ready()
    {
        base._Ready();

        Speak = GetNode<AudioStreamPlayer>("Speak");

        TextLabel = GetNode<RichTextLabel>("TextRect/MarginContainer/VBoxContainer/RichTextLabel");

        NextButton = GetNode<Button>("TextRect/MarginContainer/VBoxContainer/HBoxContainer/Next");
        SkipButton = GetNode<Button>("TextRect/MarginContainer/VBoxContainer/HBoxContainer/Skip");
        ContinueButton = GetNode<Button>("TextRect/MarginContainer/VBoxContainer/HBoxContainer/Continue");

        UpdatePages();
    }

    public void _OnNextButtonUp()
    {
        PageIndex++;
        NextButton.Visible = false;
        SkipButton.Visible = true;
        ShowCharacters();
    }

    public void _OnSkipButtonUp()
    {
        TextLabel.VisibleCharacters = TextLabel.Text.Length;
    }

    public void _OnContinueButtonUp()
    {
        EmitSignal("Done");
    }

    public void Start()
    {
        NextButton.Visible = false;
        if (PageIndex < Pages.Count)
        {
            SkipButton.Visible = true;
        }
        else
        {
            ContinueButton.Visible = true;
        }

        PageIndex = 0;
        if (Pages.Count == 0)
        {
            return;
        }

        ShowCharacters();
    }

    public async void ShowCharacters()
    {
        TextLabel.BbcodeText = Pages[PageIndex];
        TextLabel.VisibleCharacters = 0;
        while (TextLabel.VisibleCharacters < TextLabel.Text.Length)
        {
            float timeout;
            switch (TextLabel.Text[TextLabel.VisibleCharacters])
            {
                case ',':
                    timeout = CommaTime;
                    break;

                case '.':
                    timeout = PeriodTime;
                    break;

                default:
                    timeout = CharTime;
                    break;
            }
            if (TextLabel.Text[TextLabel.VisibleCharacters] != ' ')
            {
                Speak.PitchScale = (float)GD.RandRange(9, 11) / 10;
                Speak.Play();
            }
            TextLabel.VisibleCharacters++;
            await ToSignal(GetTree().CreateTimer(timeout), "timeout");
        }
        SkipButton.Visible = false;

        if (PageIndex+1 < Pages.Count)
        {
            NextButton.Visible = true;
        }
        else
        {
            ContinueButton.Visible = true;
        }
    }

    private void UpdatePages()
    {
        Pages = new Array<string>();

        string page = "";
        char lastChar = ' ';
        foreach (char c in Text)
        {
            if (lastChar == '\n' && c == '\n')
            {
                Pages.Add(page.Remove(page.Length - 1));
                page = "";
                lastChar = ' ';
                continue;
            }

            page += c;
            lastChar = c;
        }

        if (page.Length > 0)
        {
            Pages.Add(page);
        }
    }
}
