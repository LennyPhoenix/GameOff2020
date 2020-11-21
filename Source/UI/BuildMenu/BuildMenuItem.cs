using Godot;
using Godot.Collections;

public class BuildMenuItem : Button
{
    private Blueprint blueprint;
    [Export] public Blueprint Blueprint
    {
        get => blueprint;
        set
        {
            blueprint = value;

            if (TextureRect == null)
            {
                return;
            }

            Update();
        }
    }

    public AnimationPlayer AnimationPlayer;
    public TextureRect TextureRect;
    public Label NameLabel;
    public Label DescriptionLabel;

    public override void _Ready()
    {
        base._Ready();

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        TextureRect = GetNode<TextureRect>("MarginContainer/HBoxContainer/TextureRect");
        NameLabel = GetNode<Label>("Popup/Panel/VBoxContainer/Name");
        DescriptionLabel = GetNode<Label>("Popup/Panel/VBoxContainer/Description");

        if (Blueprint != null)
        {
            Update();
        }
    }

    public void _OnToggled(bool buttonPressed)
    {
        if (buttonPressed)
        {
            Array items = GetTree().GetNodesInGroup("BuildMenuItems");
            foreach (BuildMenuItem item in items)
            {
                if (item != this)
                {
                    item.Pressed = false;
                }
            }

            Globals.BuildManager.Blueprint = Blueprint;
        }
        else
        {
            if (Globals.BuildManager.Blueprint == Blueprint)
            {
                Globals.BuildManager.Blueprint = null;
            }
        }
    }

    public void _OnVisibilityChanged()
    {
        if (Pressed)
        {
            Pressed = false;
        }
    }

    public void _OnMouseEntered()
    {
        AnimationPlayer.Play("ShowName");
    }

    public void _OnMouseExited()
    {
        AnimationPlayer.Play("HideName");
    }

    private void Update()
    {
        TextureRect.Texture = Blueprint.BuildTexture;
        NameLabel.Text = Blueprint.ResourceName;
        DescriptionLabel.Text = Blueprint.Description;
    }
}
