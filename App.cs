using Godot;

public class App : Node
{
    [Export] public PackedScene PlanetScene;

    public Control MainMenu;
    public Button StartButton;

    public override void _Ready()
    {
        base._Ready();

        MainMenu = GetNode<Control>("UI/MainMenu");
        StartButton = GetNode<Button>("UI/MainMenu/StartButton");
    }

    public void _OnStartButtonUp()
    {
        MainMenu.Visible = false;

        var planet = (Planet)PlanetScene.Instance();
        AddChild(planet);
    }
}
