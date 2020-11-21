using Godot;

public class StorageItem : PanelContainer
{
    private Texture texture;
    public Texture Texture
    {
        get => texture;
        set
        {
            texture = value;

            if(TextureRect != null)
            {
                TextureRect.Texture = value;
            }
        }
    }

    private int count = 0;
    public int Count
    {
        get => count;
        set
        {
            count = value;

            if (CountLabel != null)
            {
                CountLabel.Text = value.ToString();
            }
        }
    }

    public TextureRect TextureRect;
    public Label CountLabel;

    public override void _Ready()
    {
        base._Ready();

        TextureRect = GetNode<TextureRect>("TextureRect");
        CountLabel = GetNode<Label>("TextureRect/Label");

        if (Texture != null)
        {
            TextureRect.Texture = Texture;
        }

        CountLabel.Text = Count.ToString();
    }
}
