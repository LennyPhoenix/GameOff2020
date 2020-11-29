using Godot;

public class HealthManager : Area2D
{
	[Signal] public delegate void Death();
	[Signal] public delegate void Hit(float newHealth, float maxHealth);

	[Export] public float MaxHealth = 100;
	[Export] public Shape2D Collider;
	public float Health;

	public CollisionShape2D CollisionShape2D;

	public override void _Ready()
	{
		base._Ready();

		CollisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
		Health = MaxHealth;

		if (Collider != null)
        {
			CollisionShape2D.Shape = Collider;
        }
	}

	public void _OnAreaEntered(Projectile body)
	{
        Health -= body.Damage;

		EmitSignal("Hit", Health, MaxHealth);

        if (Health <= 0)
        {
            EmitSignal("Death");
        }
    }
}
