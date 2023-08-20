using Godot;
using System;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts;

public partial class MapGrid : Node2D {
	[Export(PropertyHint.Range, "21,63,2")]
	public int MapSize = 25;

	private int _radius;
	private int _diameter;
	private const int Size = 20;
	private MapMoveLayer[,] _passiveGridLayer;
	private MapMoveLayer[,] _staticGridLayer;
	private MapMoveLayer[,] _activeGridLayer;

	public override void _Ready() {
		_radius = (MapSize - 1) / 2;
		_diameter = MapSize - 1;
		_passiveGridLayer = new MapMoveLayer[MapSize, MapSize];
		GD.Print(_passiveGridLayer[0, 0]);
	}

	public override void _Draw() {
		var center = new Vector2(_radius + 0.5f, _radius + 0.5f);
		for (int i = 0; i < _diameter + 1; i++) {
			for (int j = 0; j < _diameter + 1; j++) {
				DrawRect(new Rect2(i * Size, j * Size, Size, Size), Color.FromHtml("#FF0000"), false, 2);
			}
		}

		for (int i = 0; i < _diameter + 1; i++) {
			for (int j = 0; j < _diameter + 1; j++) {
				if (AreaHelper.IsVector2InsideCircle(new Vector2(i + 0.5f, j + 0.5f), center, _radius + 0.5f)) {
					DrawRect(new Rect2(i * Size, j * Size, Size, Size), Color.FromHtml("#0000FF"), false, 2);
				}
			}
		}
	}

	public override void _Process(double delta) {
		if (Input.IsMouseButtonPressed(MouseButton.Left)) {
			var pos = GetGlobalMousePosition();
			var gridPos = new Vector2((int)pos.X / Size, (int)pos.Y / Size);
			GD.Print(gridPos);
		}
	}
}
