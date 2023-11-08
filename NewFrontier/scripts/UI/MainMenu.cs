using Godot;
using System;

namespace NewFrontier.scripts.UI;

public partial class MainMenu : Control {

	private Button _playButton;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_playButton = GetNode<Button>("PlayButton");
		_playButton.Pressed += OnButtonPressed;
	}

	private void OnButtonPressed() {
		var loadingScene = GD.Load<PackedScene>("res://scenes/loading_scene.tscn");
		GetTree().ChangeSceneToPacked(loadingScene);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}
}
