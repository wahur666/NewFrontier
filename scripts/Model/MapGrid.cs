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
	
	/// <summary>
	/// Map representation, it can store at most 16 sectors, in a 4x4 grid of at most 128x128 sectors
	/// If there is a node, if should be a <see cref="GameNode"/> or null
	/// </summary>
	public GameNode[,] GridLayer;
	public Navigation Navigation;
	private List<WormholeObject> _wormholes = new();
	private CameraController _cameraController;

	private List<Sector> _sectors = new();

	public int RealMapSize {
		get { return MapSize * 2; }
	} 
	
	public override void _Ready() {
		_radius = MapSize - 1;
		_diameter = RealMapSize - 1;
		GridLayer = new GameNode[512, 512];
		Navigation = new(this);
		GD.Print(GridLayer[0, 0]);
		var sector = new Sector(GridLayer, new Vector2I(80, 80), (byte)MapSize, 0);
		var sector2 = new Sector(GridLayer, new Vector2I(120, 90), (byte)MapSize, 1);
		_sectors.Add(sector);
		_sectors.Add(sector2);
		_cameraController = GetNode<CameraController>("../Camera2D");
		_cameraController.Init(this);
		// _cameraController.LimitLeft = 0;
		// _cameraController.LimitTop = 0;
		// _cameraController.LimitRight = _diameter * MapHelpers.Size;
		// _cameraController.LimitBottom = _diameter * MapHelpers.Size;
		GD.Print("Camera Controller", _cameraController);
	}

	public void DrawSectors(Panel sectorMap) {
		foreach (var sector in _sectors) {
			sectorMap.DrawCircle(sector.SectorPosition, 5, Colors.Azure);
		}
	}
	
	public override void _Draw() {
		GD.Print("Drawing");
		for (int i = 0; i < GridLayer.GetLength(0); i++) {
			for (int j = 0; j < GridLayer.GetLength(1); j++) {
				if (GridLayer[i, j] is null) {
					continue;
				}
				var valid = GridLayer[i, j].Blocking;
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
			neighbours.Add(this.GridLayer[x, y]);
		}

		return neighbours;
	}

	
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
			GD.Print("Global mouse pos: ", pos);
			GD.Print("Grid pos: " + gridPos);
			GD.Print("Grid pos back to coordiante: " + MapHelpers.GridCoordToGridCenterPos(gridPos));
		}
	}

	public void SetBlocking(Vector2 origin, int size) {
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				var a = GridLayer[(int)origin.X + i, (int)origin.Y + j];
				if (a is not null) {
					a.Blocking = true;
				}
			}
		}
	}
}
