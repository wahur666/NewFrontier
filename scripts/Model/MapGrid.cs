using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public partial class MapGrid : Node2D {
	[Export(PropertyHint.Range, "21,63,2")]
	public int MapSize = 25;

	private int _radius;
	private int _diameter;
	public GameNode[,] PassiveGridLayer;
	private MapMoveLayer[,] _staticGridLayer;
	private MapMoveLayer[,] _activeGridLayer;
	public Navigation Navigation;
	private List<WormholeObject> _wormholes = new();
	private CameraController _cameraController;

	public int RealMapSize {
		get { return MapSize * 2; }
	} 

	public override void _Ready() {
		_radius = (RealMapSize - 1) / 2;
		_diameter = RealMapSize - 1;
		PassiveGridLayer = new GameNode[RealMapSize, RealMapSize];
		Navigation = new(this);
		GD.Print(PassiveGridLayer[0, 0]);
		GenerateMap();
		GenerateNeighbours();
		_cameraController = GetNode<CameraController>("../Camera2D");
		_cameraController.Init(this);
		_cameraController.LimitLeft = 0;
		_cameraController.LimitTop = 0;
		_cameraController.LimitRight = _diameter * MapHelpers.Size;
		_cameraController.LimitBottom = _diameter * MapHelpers.Size;
		GD.Print("Camera Controller", _cameraController);
	}

	private void GenerateMap() {
		var center = new Vector2(_radius + 0.5f, _radius + 0.5f);

		for (int i = 0; i < _diameter + 1; i++) {
			for (int j = 0; j < _diameter + 1; j++) {
				if (AreaHelper.IsVector2InsideCircle(new(i + 0.5f, j + 0.5f), center, _radius + 0.5f)) {
					PassiveGridLayer[i, j] = new(new(i, j));
				}
			}
		}
	}

	public override void _Draw() {
		var center = new Vector2(_radius + 0.5f, _radius + 0.5f);
		for (int i = 0; i < _diameter + 1; i++) {
			for (int j = 0; j < _diameter + 1; j++) {
				var valid = PassiveGridLayer[i, j] is null || PassiveGridLayer[i, j].Blocking;
				DrawRect(new(i * MapHelpers.Size, j * MapHelpers.Size, MapHelpers.Size, MapHelpers.Size),
					 valid ? Color.FromHtml("#FF00001A") : Color.FromHtml("#0000FF"), valid, valid ? -1 : 2);
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
			Vec2ToArray(neighboursKey.Position, out var x, out var y);
			neighbours.Add(this.PassiveGridLayer[x, y]);
		}

		return neighbours;
	}

	private void setupNeighbours(GameNode node) {
		List<Vector2> directions = new() {
			new(-1, -1),
			new(-1, 0),
			new(-1, 1),
			new(0, -1),
			new(0, 1),
			new(1, -1),
			new(1, 0),
			new(1, 1),
		};

		foreach (var direction in directions) {
			var newPos = node.Position + direction;
			if (newPos.X < 0 || newPos.Y < 0 || newPos.X > _diameter - 1 || newPos.Y > _diameter - 1) {
				continue;
			}

			var newNeighbour = PassiveGridLayer[(int)newPos.X, (int)newPos.Y];
			if (newNeighbour is not null) {
				node.AddNeighbour(newNeighbour, direction.Length());
			}
		}

		_wormholes.Where(wormhole => wormhole.IsConnected(node))
			.ToList()
			.ForEach(wormhole => node.AddNeighbour(wormhole.GetOtherNode(node), wormhole.Distance));
	}

	private void GenerateNeighbours() {
		for (var i = 0; i < _diameter; i++) {
			for (var j = 0; j < _diameter; j++) {
				var item = PassiveGridLayer[i, j];
				if (item is not null) {
					this.setupNeighbours(item);
				}
			}
		}
	}
	//
	// private createSector(x: number, y: number, size: number): void {
	//     for (let i = x; i < Math.min(this.size, x + size); i++) {
	//         for (let j = y; j < Math.min(this.size, y + size); j++) {
	//             this.sectorNodeMap[i][j] = new GameNode(new Vector2(i, j));
	//         }
	//     }
	// }
	// getNode = (x: number, y: number): GameNode => this.sectorNodeMap[x][y] as GameNode;
	//
	// private generateWormholes() {
	//     this.wormholes.push(new WormholeObject(this.getNode(8, 6), this.getNode(22, 10), 1));
	//     this.wormholes.push(new WormholeObject(this.getNode(26, 9), this.getNode(37, 14), 1));
	//     this.wormholes.push(new WormholeObject(this.getNode(25, 13), this.getNode(10, 25), 1));
	//     this.wormholes.push(new WormholeObject(this.getNode(40, 18), this.getNode(30, 30), 1));
	//     this.wormholes.push(new WormholeObject(this.getNode(16, 28), this.getNode(27, 31), 1));
	//     this.wormholes.push(new WormholeObject(this.getNode(32, 37), this.getNode(46, 32), 1));
	//     // this.wormholes.push(new WormholeObject(this.getNode(12, 20), this.getNode(12, 14), 1));
	//     // this.wormholes.push(new WormholeObject(this.getNode(20, 20), this.getNode(14, 14), 1));
	// }

	public override void _Process(double delta) {
		if (Input.IsMouseButtonPressed(MouseButton.Left)) {
			var pos = GetGlobalMousePosition();
			var gridPos = MapHelpers.PosToGrid(pos);
			GD.Print("Grid pos: " + gridPos);
			GD.Print("Grid pos back to coordiante: " + MapHelpers.GridCoordToGridCenterPos(gridPos));
		}
	}

	public void SetBlocking(Vector2 origin, int size) {
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				var a = PassiveGridLayer[(int)origin.X + i, (int)origin.Y + j];
				if (a is not null) {
					a.Blocking = true;
				}
			}
		}
	}
}
