using Godot;
using System;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts;

public partial class GasNugget : ResourceNode2D {

	private AnimationPlayer _animationPlayer;

	public GasNugget() {
		Resource = ResourceType.Gas;
	}

	public override void _Ready() {
		base._Ready();
		var startTime = GD.RandRange(0, 2.0f);
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_animationPlayer.Advance(startTime);
	}
}
