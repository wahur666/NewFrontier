using Godot;
using NewFrontier.scripts;
using System;
using System.Collections.Generic;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Controllers;

public partial class UiController : CanvasLayer
{
	
	private List<Control> canvasItems;
	private LeftControls leftControls;

	public override void _Ready() {
		canvasItems = new();
		leftControls = GetNode<LeftControls>("LeftControls");
	}

	public bool OverUiElement(Vector2 position) {
		return leftControls.OverUiElement;
	}

}
