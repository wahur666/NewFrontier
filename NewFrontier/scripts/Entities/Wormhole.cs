using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Entities;

public partial class Wormhole : Node2D {
	private Area2D _area2D;
	public bool MousePointerIsOver;
	public GameNode GameNode;

	public override void _Ready() {
		_area2D = GetNode<Area2D>("Area2D");
		_area2D.MouseEntered += Area2DOnMouseEntered;
		_area2D.MouseExited += Area2DOnMouseExited;
	}

	public void Init(GameNode gameNode) => GameNode = gameNode;

	private void Area2DOnMouseExited() => MousePointerIsOver = false;

	private void Area2DOnMouseEntered() => MousePointerIsOver = true;

	public override void _Process(double delta) {
	}
}
