using Godot;
using System;

namespace NewFrontier.scripts;

public partial class GameController : Node {

	private PackedScene _playerControllerScene;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_playerControllerScene = GD.Load<PackedScene>("res://scenes/player_controller.tscn");
		var player = _playerControllerScene.Instantiate();
		AddChild(player);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}
}
