using Godot;
using Godot.Collections;

public class GenericCarrier : Building
{
    [Export] public int CanStore = 50;
    [Export] public int CanOutput = 5;

    public override void _Ready()
    {
        MaxStorage = new Dictionary<Item, int>();
        Outputs = new Dictionary<Item, int>();

        foreach (Item item in (Item[])System.Enum.GetValues(typeof(Item)))
        {
            MaxStorage.Add(item, CanStore);
            Outputs.Add(item, CanOutput);
        }

        base._Ready();

        UpdateItemList();
    }

    public override void Tick()
    {
        base.Tick();

        UpdateItemList();
    }

    public void UpdateItemList()
    {
        if (Deleting)
        {
            return;
        }

        foreach (Item item in Items.Keys)
        {
            if (!UIStorageItems.ContainsKey(item) && Items[item] > 0)
            {
                var storageItem = (StorageItem)StorageItemScene.Instance();
                StorageGridContainer.AddChild(storageItem);
                storageItem.ItemType = item;

                UIStorageItems.Add(item, storageItem);
            }
        }

        var deleteList = new Array<Item>();

        foreach (Item item in UIStorageItems.Keys)
        {
            if (Items[item] == 0)
            {
                UIStorageItems[item].QueueFree();
                deleteList.Add(item);
            }
            else
            {
                UIStorageItems[item].Count = Items[item];
            }
        }

        foreach (Item item in deleteList)
        {
            UIStorageItems.Remove(item);
        }

        int columns = Mathf.Min(4, UIStorageItems.Count);
        int rows = Mathf.CeilToInt((float)UIStorageItems.Count / 4);
        if (columns == 0)
        {
            StorageContainer.Visible = false;
        }
        else
        {
            StorageContainer.RectSize = new Vector2(columns * 16 + 2 + Mathf.Min(columns - 1, 0), rows * 16 + 2 + Mathf.Min(rows - 1, 0));
            StorageGridContainer.Columns = columns;
        }

        if (columns == 0)
        {
            StorageContainer.Visible = false;
        }
        else
        {
            StorageContainer.Visible = true;
        }
    }
}
