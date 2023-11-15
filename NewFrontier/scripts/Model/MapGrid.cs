using System.Collections.Generic;
using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public partial class MapGrid : Node2D {
	private CameraController _cameraController;
	private int _diameter;

	private int _radius;
	private readonly List<WormholeObject> _wormholes = new();
	private PackedScene _wormholeScene;


	/// <summary>
	///     Map representation, it can store at most 16 sectors, in a 4x4 grid of at most 128x128 sectors
	///     If there is a node, if should be a <see cref="GameNode" /> or null
	/// </summary>
	public GameNode[,] GridLayer;

	[Export(PropertyHint.Range, "21,63,2")]
	public int MapSize = 25;

	public Navigation Navigation;

	public List<Sector> Sectors = new();

	public List<GameNode> WormholeNodes = new();

	public int RealMapSize => MapSize * 2;

	public override void _Ready() {
		_radius = MapSize - 1;
		_diameter = RealMapSize - 1;
		_wormholeScene = GD.Load<PackedScene>("res://scenes/wormhole.tscn");
		GridLayer = new GameNode[512, 512];
		Navigation = new Navigation(this);
		GD.Print(GridLayer[0, 0]);
		var sector = new Sector(GridLayer, new Vector2I(80, 80), (byte)MapSize, 0);
		var sector2 = new Sector(GridLayer, new Vector2I(120, 90), (byte)MapSize, 1);
		Sectors.Add(sector);
		Sectors.Add(sector2);
		_cameraController = GetNode<CameraController>("../Camera2D");
		_cameraController.Init(this);
		// _cameraController.LimitLeft = 0;
		// _cameraController.LimitTop = 0;
		// _cameraController.LimitRight = _diameter * MapHelpers.Size;
		// _cameraController.LimitBottom = _diameter * MapHelpers.Size;
		GD.Print("Camera Controller", _cameraController);
	}

	public override void _Draw() {
		GD.Print("Drawing");
		for (var i = 0; i < GridLayer.GetLength(0); i += 2) {
			for (var j = 0; j < GridLayer.GetLength(1); j += 2) {
				if (GridLayer[i, j] is null) {
					continue;
				}

				var valid = GridLayer[i, j].Blocking;
				DrawRect(
					new Rect2(i * MapHelpers.DrawSize, j * MapHelpers.DrawSize, MapHelpers.DrawSize * 2,
						MapHelpers.DrawSize * 2),
					valid ? Color.FromHtml("#FF00001A") : Color.FromHtml("#0000FF"), valid, valid ? -1 : 2);
			}
		}
	}

	private static void Vec2ToArray(Vector2 vector2, out int x, out int y) {
		x = (int)vector2.X;
		y = (int)vector2.Y;
	}

	public override void _Process(double delta) {
		// if (Input.IsMouseButtonPressed(MouseButton.Left)) {
		// 	var pos = GetGlobalMousePosition();
		// 	var gridPos = MapHelpers.PosToGrid(pos);
		// 	GD.Print("Global mouse pos: ", pos);
		// 	GD.Print("Grid pos: " + gridPos);
		// 	GD.Print("Grid pos back to coordiante: " + MapHelpers.GridCoordToGridCenterPos(gridPos));
		// }
	}

	public void SetBlocking(Vector2 origin, int size) {
		for (var i = 0; i < size; i++) {
			for (var j = 0; j < size; j++) {
				var a = GridLayer[(int)origin.X + i, (int)origin.Y + j];
				if (a is not null) {
					a.Blocking = true;
				}
			}
		}
	}


	public void CreateWormholes(Vector2 position1, int sector1, Vector2 position2, int sector2) {
		var pos1 = MapHelpers.CalculateOffset(position1, sector1);
		var pos2 = MapHelpers.CalculateOffset(position2, sector2);
		var w1 = _wormholeScene.Instantiate<Node2D>();
		w1.Position = MapHelpers.GridCoordToGridPointPos(pos1);
		AddChild(w1);
		var w2 = _wormholeScene.Instantiate<Node2D>();
		w2.Position = MapHelpers.GridCoordToGridPointPos(pos2);
		AddChild(w2);

		var w1Node = SetupWormholeGameNode(position1, sector1);
		var w2Node = SetupWormholeGameNode(position2, sector2);
		w1Node.AddNeighbourTwoWays(w2Node, 1);

		_wormholes.Add(new WormholeObject(w1Node, w2Node));
	}

	private GameNode SetupWormholeGameNode(Vector2 position, int sector) {
		var offset = MapHelpers.CalculateOffset(position, sector);
		var gameNode = new GameNode(offset - new Vector2(0.5f, 0.5f)) {
			HasWormhole = true
		};

		ConnectNeighbours(offset, gameNode, -2, -2);
		ConnectNeighbours(offset, gameNode, -1, -2);
		ConnectNeighbours(offset, gameNode, 0, -2);
		ConnectNeighbours(offset, gameNode, 1, -2);

		ConnectNeighbours(offset, gameNode, -2, -1);
		ConnectNeighbours(offset, gameNode, -2, 0);

		ConnectNeighbours(offset, gameNode, 1, -1);
		ConnectNeighbours(offset, gameNode, 1, 0);

		ConnectNeighbours(offset, gameNode, -2, 1);
		ConnectNeighbours(offset, gameNode, -1, 1);
		ConnectNeighbours(offset, gameNode, 0, 1);
		ConnectNeighbours(offset, gameNode, 1, 1);

		WormholeNodes.Add(gameNode);

		SetWormhole(offset, -1, -1, gameNode);
		SetWormhole(offset, -1, 0, gameNode);
		SetWormhole(offset, 0, -1, gameNode);
		SetWormhole(offset, 0, 0, gameNode);

		return gameNode;
	}

	private void ConnectNeighbours(Vector2 offset, GameNode gameNode, int offsetX, int offsetY) {
		var tmpNode = GridLayer[(int)(offset.X + offsetX), (int)(offset.Y + offsetY)];
		tmpNode?.AddNeighbourTwoWays(gameNode, tmpNode.Distance(gameNode));
	}

	private void SetWormhole(Vector2 offset, int offsetX, int offsetY, GameNode gameNode) {
		GridLayer[(int)(offset.X + offsetX), (int)(offset.Y + offsetY)]?.SetWormhole(gameNode);
	}


	public GameNode GetGameNode(Vector2 positon, int sector) {
		var offset = MapHelpers.CalculateOffset(positon, sector);
		return GridLayer[offset.X, offset.Y];
	}

	public GameNode GetGameNode(Vector2 positon) {
		return GridLayer[(int)positon.X, (int)positon.Y];
	}
}
