using Godot;

public class App : Node
{
    [Export] public PackedScene PlanetScene;

    public Control MainMenu;
    public Button StartButton;

    public Planet Planet;

    public override void _Ready()
    {
        base._Ready();

        MainMenu = GetNode<Control>("UI/MainMenu");
        StartButton = GetNode<Button>("UI/MainMenu/StartButton");
    }

    public void _OnQuit()
    {
        Planet = null;

        MainMenu.Visible = true;
    }

    public void _OnStartButtonUp()
    {
        MainMenu.Visible = false;

        Planet = (Planet)PlanetScene.Instance();
        AddChild(Planet);

        Planet.Connect("Quit", this, "_OnQuit");
    }
}
