using Godot;
using System;
using System.Collections.Generic;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public partial class MapGrid : Node2D {
	[Export(PropertyHint.Range, "21,63,2")]
	public int MapSize = 25;

	private int _radius;
	private int _diameter;
	public const int Size = 80;
	public GameNode[,] PassiveGridLayer;
	private MapMoveLayer[,] _staticGridLayer;
	private MapMoveLayer[,] _activeGridLayer;

	public override void _Ready() {
		_radius = (MapSize - 1) / 2;
		_diameter = MapSize - 1;
		PassiveGridLayer = new GameNode[MapSize, MapSize];
		GD.Print(PassiveGridLayer[0, 0]);
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

	private static void Vec2ToArray(Vector2 vector2, out int x, out int y) {
		x = (int)vector2.X;
		y = (int)vector2.Y;
	}

	
	public List<GameNode> NodeNeighbours(GameNode node) {
		var neighbours = new List<GameNode>();
		foreach (var neighboursKey in node.Neighbours.Keys) {
			int x, y;
			Vec2ToArray(neighboursKey.Position, out x, out y);
			neighbours.Add(this.PassiveGridLayer[x, y]);
		}
		return neighbours;
	}
	
	public override void _Process(double delta) {
		if (Input.IsMouseButtonPressed(MouseButton.Left)) {
			var pos = GetGlobalMousePosition();
			var gridPos = MapHelpers.PosToGrid(pos, Size);
			GD.Print("Grid pos: " +  gridPos);
			GD.Print("Grid pos back to coordiante: " +  MapHelpers.GridCoordToGridCenterPos(gridPos, Size));
		}
	}
}
