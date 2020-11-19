using Godot;
using Godot.Collections;

[Tool]
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
            Label.Text = Blueprint.ResourceName;
        }
    }

    public TextureRect TextureRect;
    public Label Label;
    public BuildPreview BuildPreview;

    public override void _Ready()
    {
        base._Ready();

        TextureRect = GetNode<TextureRect>("MarginContainer/VBoxContainer/TextureRect");
        Label = GetNode<Label>("MarginContainer/VBoxContainer/Label");

        if (Blueprint != null)
        {
            TextureRect.Texture = Blueprint.BuildTexture;
            Label.Text = Blueprint.ResourceName;
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
}
