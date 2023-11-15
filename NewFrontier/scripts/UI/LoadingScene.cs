using System;
using Godot;
using GDArray = Godot.Collections.Array;

namespace NewFrontier.scripts.UI;

public partial class LoadingScene : Control {
	private Label _label;
	private string _level = "res://scenes/level.tscn";
	private float _sceneLoadStatus;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_label = GetNode<Label>("Label");
		ResourceLoader.LoadThreadedRequest(_level);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		GDArray progress = new();
		var status = ResourceLoader.LoadThreadedGetStatus(_level, progress);
		_label.Text = $"Loading: {Math.Round(progress[0].AsSingle() * 100)}%";

		if (status != ResourceLoader.ThreadLoadStatus.Loaded) {
			return;
		}

		var timer = new Timer();
		timer.Timeout += SwitchScene;
		AddChild(timer);
		timer.Start(2);
	}

	private void SwitchScene() {
		var newScene = ResourceLoader.LoadThreadedGet(_level) as PackedScene;
		GetTree().ChangeSceneToPacked(newScene);
	}
}
