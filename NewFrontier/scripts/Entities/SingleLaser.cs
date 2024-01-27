using Godot;

namespace NewFrontier.scripts.Entities;

public partial class SingleLaser : Area2D {
	[Export] public float Speed = 400f;
	private Timer _timer;

	public override void _Ready() {
		_timer = GetNode<Timer>("Timer");
		_timer.Timeout += TimerOnTimeout;
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);
		Vector2 velocity = CalculateVelocity(Rotation);
		Position += velocity * Speed * (float)delta;
	}

	private void TimerOnTimeout() {
		QueueFree();
	}

	private static Vector2 CalculateVelocity(float rotation) {
		// Convert rotation to a unit vector
		Vector2 velocity = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
		velocity = velocity.Normalized(); // Ensure the vector is normalized (length of 1)
		return velocity;
	}
}
