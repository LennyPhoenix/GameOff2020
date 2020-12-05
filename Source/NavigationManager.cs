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
    public Vector2[] LastPath;

    private Dictionary<Node2D, NavQueueItem> navQueue = new Dictionary<Node2D, NavQueueItem>();

    private Thread pathThread;
    private bool run = true;

    public override void _Ready()
    {
        base._Ready();

        pathThread = new Thread();

        Start();
    }

    public void QueueAgent(Node2D agent, Vector2 targetLocation, Action<Godot.Collections.Array<Vector2>> callback)
    {
        if (navQueue.ContainsKey(agent))
        {
            return;
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

                Vector2[] points;
                if (LastAgent != null && IsInstanceValid(LastAgent) && LastTarget.Round() == queueItem.Target.Round() && LastAgent.GlobalPosition.DistanceTo(nextAgent.GlobalPosition) < 256)
                {
                    points = LastPath;
                }
                else
                {
                    GD.Print("Calculating.");
                    points = GetSimplePath(nextAgent.GlobalPosition, queueItem.Target);
                }

                var newPath = new Godot.Collections.Array<Vector2>();
                foreach (Vector2 point in points)
                {
                    newPath.Add(point);
                }

                navQueue.Remove(nextAgent);

                LastAgent = nextAgent;
                LastTarget = queueItem.Target;
                LastPath = points;

                queueItem.Callback(newPath);
            }
        }

        pathThread.CallDeferred("WaitToFinish");
    }
}
