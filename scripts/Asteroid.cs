using Godot;
using System;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts;

public partial class Asteroid : ResourceNode2D {
	private AnimatedSprite2D _animatedSprite2D;

	public Asteroid() {
		Resource = ResourceType.Ore;
	}

	public override void _Ready() {
		base._Ready();
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_animatedSprite2D.RotationDegrees = GD.RandRange(0, 360);
	}
}
