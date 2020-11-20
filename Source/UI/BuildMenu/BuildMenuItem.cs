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

            TextureRect.Texture = Blueprint.BuildTexture;
            NameLabel.Text = Blueprint.ResourceName;
        }
    }

    public AnimationPlayer AnimationPlayer;
    public TextureRect TextureRect;
    public Label NameLabel;
    public BuildPreview BuildPreview;

    public override void _Ready()
    {
        base._Ready();

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        TextureRect = GetNode<TextureRect>("MarginContainer/TextureRect");
        NameLabel = GetNode<Label>("Name");

        if (Blueprint != null)
        {
            TextureRect.Texture = Blueprint.BuildTexture;
            NameLabel.Text = Blueprint.ResourceName;
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

            BuildPreview.Blueprint = Blueprint;
        }
        else
        {
            if (BuildPreview.Blueprint == Blueprint)
            {
                BuildPreview.Blueprint = null;
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
}
