using Godot;

// Ported to C# from https://godotengine.org/qa/438/camera2d-screen-shake-extension

public class ShakeCamera2D : Camera2D
{
    private float duration = 0f;
    private float periodInMs = 0f;
    private float amplitude = 0f;
    private float timer = 0f;
    private float lastShootTimer = 0f;
    private float previousX = 0f;
    private float previousY = 0f;
    private Vector2 lastOffset = Vector2.Zero;

    // Shake with decreasing intensity while there's time remaining.
    public override void _Process(float delta) {
        // Only shake when there's shake time remaining.
        if (timer == 0) 
        {
            return;
        }

        // Only shake on certain frames.
        lastShootTimer += delta;

        // Be mathematically correct in the face of lag; usually only happens once.
        while (lastShootTimer >= periodInMs) 
        {
            lastShootTimer -= periodInMs;

            // Lerp between [amplitude] and 0.0 intensity based on remaining shake time.
            float intensity = amplitude * (1 - ((duration - timer) / duration));

            // Noise calculation logic from http://jonny.morrill.me/blog/view/14
            float newX = (float)GD.RandRange(-1, 1);
            float xComponent = intensity * (previousX + (delta * (newX - previousX)));
            float newY = (float)GD.RandRange(-1, 1);
            float yComponent = intensity * (previousY + (delta * (newY - previousY)));

            previousX = newX;
            previousY = newY;

            // Track how much we've moved the offset, as opposed to other effects.
            Vector2 newOffset = new Vector2(xComponent, yComponent);
            Offset += -lastOffset + newOffset;
            lastOffset = newOffset;
        }

        // Reset the offset when we're done shaking.
        timer -= delta;
        if (timer <= 0) 
        {
            timer = 0;
            Offset -= lastOffset;
        }
    }

    // Kick off a new screenshake effect.
    public void Shake(float duration, float frequency, float amplitude, Vector2 rootPosition) {
        // Initialize variables.
        this.duration = duration;
        timer = duration;
        periodInMs = 1f / frequency;
        this.amplitude = amplitude / Mathf.Max(1, rootPosition.DistanceTo(GlobalPosition) / 128);
        previousX = (float)GD.RandRange(-1, 1);
        previousY = (float)GD.RandRange(-1, 1);

        // Reset previous offset, if any.
        Offset -= lastOffset;
        lastOffset = Vector2.Zero;
    }
}
