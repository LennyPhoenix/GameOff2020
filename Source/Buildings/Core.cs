using Godot;
using Godot.Collections;

public class Core : GenericCarrier
{
    [Export] public NodePath ResourcesUIPath;

    public Dictionary<Item, StorageItem> ResourcesUIItems = new Dictionary<Item, StorageItem>();

    public override void _EnterTree()
    {
        base._EnterTree();

        Globals.Core = this;
    }

    public override void _Ready()
    {
        base._Ready();

        Items[Item.Stone] = 80;
        Items[Item.Copper] = 80;
    }

    public override void Tick()
    {
        base.Tick();

        if (ResourcesUIPath != null)
        {
            int columns = Mathf.Max(1, UIStorageItems.Count);
            var resourcesUI = GetNode<NinePatchRect>(ResourcesUIPath);
            var gridContainer = resourcesUI.GetNode<GridContainer>("MarginContainer/GridContainer");

            int width = columns * 16 + 3 * Mathf.Min(columns - 1, 0) + 17 * 2;
            gridContainer.Columns = columns;

            resourcesUI.MarginLeft = -width / 2;
            resourcesUI.MarginRight = width / 2;

            foreach (Item item in UIStorageItems.Keys)
            {
                if (!ResourcesUIItems.ContainsKey(item))
                {
                    var texture = ResourceLoader.Load<Texture>("res://Assets/Items/" + item.ToString() + ".png");
                    var storageItem = (StorageItem)StorageItemScene.Instance();
                    gridContainer.AddChild(storageItem);
                    storageItem.Texture = texture;

                    ResourcesUIItems.Add(item, storageItem);
                }
            }

            Array<Item> deleteList = new Array<Item>();

            foreach (Item item in ResourcesUIItems.Keys)
            {
                if (Items[item] == 0)
                {
                    ResourcesUIItems[item].QueueFree();
                    deleteList.Add(item);
                }
                else
                {
                    ResourcesUIItems[item].Count = Items[item];
                }
            }

            foreach (Item item in deleteList)
            {
                ResourcesUIItems.Remove(item);
            }
        }
    }
}
