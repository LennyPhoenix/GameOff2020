using Godot;
using Godot.Collections;

public class BuildMenuItem : Button
{
    [Export] public PackedScene StorageItemScene;

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

            UpdateFields();
        }
    }

    public AnimationPlayer AnimationPlayer;
    public TextureRect TextureRect;
    public GridContainer GridContainer;
    public Label NameLabel;
    public Label DescriptionLabel;

    public override void _Ready()
    {
        base._Ready();

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        TextureRect = GetNode<TextureRect>("MarginContainer/VBoxContainer/TextureRect");
        GridContainer = GetNode<GridContainer>("MarginContainer/VBoxContainer/GridContainer");
        NameLabel = GetNode<Label>("Popup/Panel/VBoxContainer/Name");
        DescriptionLabel = GetNode<Label>("Popup/Panel/VBoxContainer/Description");

        if (Blueprint != null)
        {
            UpdateFields();
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

    private void UpdateFields()
    {
        TextureRect.Texture = Blueprint.BuildTexture;
        NameLabel.Text = Blueprint.ResourceName;
        DescriptionLabel.Text = Blueprint.Description;

        foreach (Item item in Blueprint.Cost.Keys)
        {
            var storageItem = (StorageItem)StorageItemScene.Instance();
            GridContainer.AddChild(storageItem);
            storageItem.ItemType = item;
            storageItem.Count = Blueprint.Cost[item];
        }
    }
}
