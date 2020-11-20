using Godot;

public class StorageItem : PanelContainer
{
    public Texture Texture
    {
        get => TextureRect.Texture;
        set
        {
            TextureRect.Texture = value;
        }
    }

    public int Count
    {
        get { return CountLabel.Text.ToInt(); }
        set
        {
            CountLabel.Text = value.ToString();
        }
    }

    public TextureRect TextureRect;
    public Label CountLabel;

    public override void _Ready()
    {
        base._Ready();

        TextureRect = GetNode<TextureRect>("TextureRect");
        CountLabel = GetNode<Label>("TextureRect/Label");

        Count = 0;
    }
}
