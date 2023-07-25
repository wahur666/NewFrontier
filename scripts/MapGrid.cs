using Godot;
using System;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts;

public partial class MapGrid : Node2D {
	const int size = 20;
	const int radius = 12;
	const int diameter = radius * 2;
	public override void _Draw() {
		var center = new Vector2(radius + 0.5f, radius + 0.5f);
		for (int i = 0; i < diameter + 1; i++) {
			for (int j = 0; j < diameter + 1; j++) {
				DrawRect(new Rect2(i * size, j * size, size, size), Color.FromHtml("#FF0000"), false, 2);
			}
		}

		for (int i = 0; i < diameter + 1; i++) {
			for (int j = 0; j < diameter + 1; j++) {
				if (CircleHelper.IsVector2InsideCircle(new Vector2(i + 0.5f, j + 0.5f), center, radius + 0.5f))
					DrawRect(new Rect2(i * size, j * size, size, size), Color.FromHtml("#0000FF"), false, 2);
			}
		}
	}

    public override void _Process(double delta) {
		if (Input.IsMouseButtonPressed(MouseButton.Left)) {
			var pos = GetGlobalMousePosition();
			var gridPos = new Vector2((int)pos.X / size, (int)pos.Y / size);
			GD.Print(gridPos);
		}
    }	
}
