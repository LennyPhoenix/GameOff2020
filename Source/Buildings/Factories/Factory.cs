using Godot;
using Godot.Collections;

public class Factory : Building
{
    [Export] public Array<Recipe> Recipes = new Array<Recipe>();

    private bool running = false;
    public bool Running
    {
        get => running;
        set
        {
            running = value;

            if (value && SpriteAnimationPlayer.AssignedAnimation == "Off")
            {
                SpriteAnimationPlayer.Play("PowerUp");
            }
            else if (value && SpriteAnimationPlayer.AssignedAnimation == "PowerDown")
            {
                SpriteAnimationPlayer.Play("On");
            }
        }
    }

    public override void _Ready()
    {
        foreach (Recipe recipe in Recipes)
        {
            foreach (Item item in recipe.Costs.Keys)
            {
                if (!MaxStorage.ContainsKey(item))
                {
                    MaxStorage.Add(item, recipe.Costs[item]);
                }
                else
                {
                    MaxStorage[item] = Mathf.Max(MaxStorage[item], recipe.Costs[item]);
                }
            }

            foreach (Item item in recipe.Produces.Keys)
            {
                if (!MaxStorage.ContainsKey(item))
                {
                    MaxStorage.Add(item, recipe.Produces[item]);
                }
                else
                {
                    MaxStorage[item] = Mathf.Max(MaxStorage[item], recipe.Produces[item]);
                }

                if (!Outputs.ContainsKey(item))
                {
                    Outputs.Add(item, recipe.Produces[item]);
                }
                else
                {
                    Outputs[item] = Mathf.Max(Outputs[item], recipe.Produces[item]);
                }
            }
        }

        base._Ready();
    }

    public override void Tick()
    {
        bool ran = false;
        foreach (Recipe recipe in Recipes)
        {
            bool canRun = true;
            foreach (Item item in recipe.Costs.Keys)
            {
                if (recipe.Costs[item] > Items[item])
                {
                    canRun = false;
                    break;
                }
            }

            foreach (Item item in recipe.Produces.Keys)
            {
                if (MaxStorage[item] - Items[item] < recipe.Produces[item])
                {
                    canRun = false;
                    break;
                }
            }

            if (canRun)
            {
                foreach (Item item in recipe.Costs.Keys)
                {
                    Items[item] -= recipe.Costs[item];
                }

                foreach (Item item in recipe.Produces.Keys)
                {
                    Items[item] += recipe.Produces[item];
                }

                ran = true;

                break;
            }
        }

        Running = ran;

        base.Tick();
    }

    public void _OnSpriteAnimationFinished(string animName)
    {
        if (animName == "PowerUp")
        {
            SpriteAnimationPlayer.Play("On");
        }
        else if (animName == "PowerDown")
        {
            SpriteAnimationPlayer.Play("Off");
        }
        else if (animName == "On")
        {
            if (Running)
            {
                SpriteAnimationPlayer.Play("On");
            }
            else
            {
                SpriteAnimationPlayer.Play("PowerDown");
            }
        }
    }
}
