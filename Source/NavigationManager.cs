using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public struct NavQueueItem
{
    public Vector2 Target;
    public Action<Godot.Collections.Array<Vector2>> Callback;
}

public class NavigationManager : Navigation2D
{
    public Node2D LastAgent;
    public Vector2 LastTarget;
    public Godot.Collections.Array<Vector2> LastPath;

    private Dictionary<Node2D, NavQueueItem> navQueue = new Dictionary<Node2D, NavQueueItem>();

    private Thread pathThread = new Thread();
    private bool run = true;

    public override void _Ready()
    {
        base._Ready();

        Start();
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        Stop();
    }

    public void QueueAgent(Node2D agent, Vector2 targetLocation, Action<Godot.Collections.Array<Vector2>> callback)
    {
        if (navQueue.ContainsKey(agent))
        {
            if (navQueue[agent].Target.Round() != targetLocation.Round())
            {
                navQueue.Remove(agent);
            }
            else
            {
                return;
            }
        }

        var queueItem = new NavQueueItem()
        {
            Target = targetLocation,
            Callback = callback
        };

        navQueue.Add(agent, queueItem);
    }

    public void RemoveAgent(Node2D agent)
    {
        if (navQueue.ContainsKey(agent))
        {
            navQueue.Remove(agent);
        }
    }

    public void Start()
    {
        if (!pathThread.IsActive())
        {
            pathThread.Start(this, "CalculatePaths");
        }
        run = true;
    }

    public void Stop()
    {
        run = false;
    }

    public void CalculatePaths(object userData)
    {
        while (run)
        {
            if (navQueue.Count > 0)
            {
                Node2D nextAgent = navQueue.Keys.First();
                NavQueueItem queueItem = navQueue[nextAgent];

                Godot.Collections.Array<Vector2> newPath;
                if (LastAgent != null && IsInstanceValid(LastAgent) && LastTarget.Round() == queueItem.Target.Round() && LastAgent.GlobalPosition.DistanceTo(nextAgent.GlobalPosition) < 256)
                {
                    newPath = LastPath;
                }
                else
                {
                    Vector2[] points = GetSimplePath(nextAgent.GlobalPosition, queueItem.Target);

                    newPath = new Godot.Collections.Array<Vector2>();
                    foreach (Vector2 point in points)
                    {
                        newPath.Add(point);
                    }
                }

                navQueue.Remove(nextAgent);

                LastAgent = nextAgent;
                LastTarget = queueItem.Target;
                LastPath = newPath;

                queueItem.Callback(newPath);
            }
        }

        pathThread.CallDeferred("WaitToFinish");
    }
}
