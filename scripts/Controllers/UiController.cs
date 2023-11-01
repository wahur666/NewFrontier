using Godot;
using NewFrontier.scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Controllers;

public partial class UiController : CanvasLayer {
	private List<Control> canvasItems;
	private LeftControls leftControls;
	public Node2D Canvas;
	private PlayerController _playerController;

	public override void _Ready() {
		canvasItems = new();
		leftControls = GetNode<LeftControls>("LeftControls");
		Canvas = GetNode<Node2D>("../Canvas");
	}

	public void Init(PlayerController playerController) {
		_playerController = playerController;
		leftControls.Init(playerController);
	}

	public bool OverUiElement(Vector2 position) {
		return leftControls.OverUiElement;
	}

	public override void _Process(double delta) {
		Canvas.QueueRedraw();
	}
}
