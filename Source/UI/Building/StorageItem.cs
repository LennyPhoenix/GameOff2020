using Godot;

public class StorageItem : PanelContainer
{
    private Item item;
    public Item ItemType
    {
        get => item;
        set
        {
            item = value;

            if (TextureRect != null)
            {
                var texture = ResourceLoader.Load<Texture>("res://Assets/Items/" + item.ToString() + ".png");
                TextureRect.Texture = texture;
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

    private float countFloat = 0f;
    public float CountFloat
    {
        get => countFloat;
        set
        {
            countFloat = value;

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

        if (ItemType != null)
        {
            var texture = ResourceLoader.Load<Texture>("res://Assets/Items/" + item.ToString() + ".png");
            TextureRect.Texture = texture;
        }

        if (CountFloat == 0)
        {
            CountLabel.Text = Count.ToString();
        }
        else
        {
            CountLabel.Text = CountFloat.ToString();
        }
    }
}
