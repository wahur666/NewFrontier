using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.Entities;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public partial class MapGrid : Node2D {
	public readonly List<Planet> Planets = [];
	public readonly List<WormholeObject> WormholeObjects = [];
	public readonly List<Wormhole> Wormholes = [];
	private CameraController _cameraController;
	private int _diameter;

	private int _radius;
	private PackedScene _wormholeScene;
	private static Vector2[] fourDirections = [Vector2.Zero, new Vector2(-1, 0), new Vector2(0, -1), -Vector2.One];



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
	private Timer _redrawTimer;

	public int RealMapSize => MapSize * 2;

	public GameNode this[int vector2X, int vector2Y] => GridLayer[vector2X, vector2Y];

	public override void _Ready() {
		_radius = MapSize - 1;
		_diameter = RealMapSize - 1;
		_wormholeScene = GD.Load<PackedScene>("res://scenes/wormhole.tscn");
		GridLayer = new GameNode[512, 512];
		Navigation = new Navigation(this);
		var sector = new Sector(GridLayer, new Vector2I(80, 80), (byte)MapSize, 0);
		Sectors.Add(sector);
		// TODO: remove manual assignment
		var sector2 = new Sector(GridLayer, new Vector2I(120, 90), (byte)MapSize, 1) {
			Discovered = false,
			SectorBuildingStatus = SectorBuildingStatus.HasBuilding,
			EnemyInSector = true
		};
		Sectors.Add(sector2);
		_cameraController = GetNode<CameraController>("../Camera2D");
		_cameraController.Init(this);
		_redrawTimer = GetNode<Timer>("Timer");
		_redrawTimer.Timeout += QueueRedraw;
	}

	public override void _Draw() {
		for (var i = 0; i < GridLayer.GetLength(0); i += 1) {
			for (var j = 0; j < GridLayer.GetLength(1); j += 1) {
				if (GridLayer[i, j] is null) {
					continue;
				}

				var valid = GridLayer[i, j].Blocking;
				DrawRect(
					new Rect2(i * MapHelpers.DrawSize, j * MapHelpers.DrawSize, MapHelpers.DrawSize,
						MapHelpers.DrawSize),
					valid ? Color.FromHtml("#FF00001A") : Color.FromHtml("#0000FF"), valid, valid ? -1 : 2);
				if (GridLayer[i, j].Occupied) {
					DrawRect(
						new Rect2(i * MapHelpers.DrawSize, j * MapHelpers.DrawSize, MapHelpers.DrawSize,
							MapHelpers.DrawSize), Color.FromHtml("#EEF5FF1A"));
				}
				
				if (GridLayer[i, j].PreOccupied is not null) {
					DrawRect(
						new Rect2(i * MapHelpers.DrawSize, j * MapHelpers.DrawSize, MapHelpers.DrawSize,
							MapHelpers.DrawSize), Color.FromHtml("#176B875A"));
				}
			}
		}
	}

	public void SetBlocking(Vector2 origin, int size) {
		for (var i = 0; i < size; i++) {
			for (var j = 0; j < size; j++) {
				var a = GridLayer[(int)origin.X + i, (int)origin.Y + j];
				if (a is not null) {
					a.Blocking = true;
					// GD.Print($"Blocking tile: {a.PositionI}");
				}
			}
		}
	}


	public void CreateWormholes(Vector2 position1, int sector1, Vector2 position2, int sector2) {
		var pos1 = MapHelpers.CalculateOffset(position1, sector1);
		var pos2 = MapHelpers.CalculateOffset(position2, sector2);
		var w1 = _wormholeScene.Instantiate<Wormhole>();
		w1.Position = MapHelpers.GridCoordToGridPointPos(pos1);
		AddChild(w1);
		var w2 = _wormholeScene.Instantiate<Wormhole>();
		w2.Position = MapHelpers.GridCoordToGridPointPos(pos2);
		AddChild(w2);

		var w1Node = SetupWormholeGameNode(position1, sector1);
		w1.Init(w1Node);
		var w2Node = SetupWormholeGameNode(position2, sector2);
		w2.Init(w2Node);
		w1Node.AddNeighbourTwoWays(w2Node, 1);
		Wormholes.Add(w1);
		Wormholes.Add(w2);
		WormholeObjects.Add(new WormholeObject(w1Node, w2Node));
	}

	public GameNode GetConnectedWormholeNode(GameNode gameNode) {
		return WormholeObjects.Find(x => x.IsConnected(gameNode)).GetOtherNode(gameNode);
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

	public Sector GetSector(byte index) {
		return Sectors.Find(x => x.Index == index);
	}

	public GameNode GetGameNode(Vector2 positon, int sector) {
		var offset = MapHelpers.CalculateOffset(positon, sector);
		return GridLayer[offset.X, offset.Y];
	}

	public GameNode GetGameNode(Vector2 positon) {
		return GridLayer[(int)positon.X, (int)positon.Y];
	}

	public Vector2? FreeSpace(Vector2 mousePos, int wide, bool wormhole = false) {
		var gridPos = MapHelpers.PosToGrid(mousePos);
		if (wide == 1) {
			var node = GetGameNode(gridPos);
			return !node.Occupied && !node.Blocking && !node.HasWormhole ? gridPos : null;
		}

		if (wide == 2) {
			if (gridPos.X % 2 != 1) {
				gridPos += new Vector2I(1, 0);
			}

			if (gridPos.Y % 2 != 1) {
				gridPos += new Vector2I(0, 1);
			}

			var allGirdSpacesFree = fourDirections
				.Select(direction => gridPos + direction)
				.Select(vector2 => GetGameNode(vector2))
				.All(node => !node.Occupied && !node.Blocking && node.HasWormhole == wormhole);
			return allGirdSpacesFree ? gridPos : null;
		}

		return null;
	}

	public List<Wormhole> GetWormholes(Vector2 gridPos) {
		var node = GetGameNode(gridPos).WormholeNode;
		var node2 = GetConnectedWormholeNode(node);

		var w1 = Wormholes.Find(wormhole => wormhole.GameNode == node);
		var w2 = Wormholes.Find(wormhole => wormhole.GameNode == node2);
		return [w1, w2];
	}

	public IEnumerable<GameNode> GetWormholeOccupiedNodes(List<Wormhole> wormholes) {
		List<GameNode> nodes = [];

		foreach (var wormhole in wormholes) {
			var pos = wormhole.GameNode.Position;
			pos = new Vector2(Mathf.Ceil(pos.X), Mathf.Ceil(pos.Y));

			nodes.AddRange(fourDirections.Select(direction => direction + pos).Select(vector2 => GetGameNode(vector2)));
		}

		return nodes;
	}

	public void ClearUnitPreOccupation(UnitNode2D unitNode2D) {
		for (var i = 0; i < GridLayer.GetLength(0); i += 1) {
			for (var j = 0; j < GridLayer.GetLength(1); j += 1) {
				if (GridLayer[i, j] is not null && GridLayer[i, j].PreOccupied == unitNode2D) {
					GridLayer[i, j].PreOccupied = null;
				}
			}
		}
	}

	public List<GameNode> FindFreePosition(GameNode end, bool bigShip) {
		List<GameNode> nodes = GetSector(end.SectorIndex).SectorGameNodes().ToList();
		nodes.Sort((a, b) => a.Distance(end).CompareTo(b.Distance(end)));

		if (bigShip) {
			foreach (var node in nodes) {
				var pos = node.PositionI;
				List<List<Vector2I>> directions = [
					[new(-1, -1), new(-1, 0), new(0, -1), new(0, 0)],
					[new(-1, 0), new(-1, 1), new(0, 0), new(0, 1)],
					[new(0, -1), new(0, 0), new(1, -1), new(1, 0)],
					[new(0, 0), new(0, 1), new(1, 0), new(1, 1)]
				];

				foreach (var nodez in directions.Select(direction =>
							 direction.Select(x => x + pos).Select(x => this[x.X, x.Y]).ToList())) {
					if (nodez.All(x => x is not null && x.FreeNode())) {
						return nodez;
					}
				}
			}
		} else {
			var node = nodes.FirstOrDefault(node => node.FreeNode());
			return node is not null ? [node] : [];
		}

		return [];
	}
}
