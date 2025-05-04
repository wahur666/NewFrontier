using System.Collections.Generic;
using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public class Sector {
	/// <summary>
	///     GameMap object
	/// </summary>
	private readonly GameNode[,] _map;

	/// <summary>
	///     Size of the Sector max 128
	/// </summary>
	private readonly byte _size;

	public Vector2 CameraPosition;
	public bool Discovered = true;

	public bool EnemyInSector;

	/// <summary>
	///     Index if the sector, to allocate the memory, 0-15
	/// </summary>
	public byte Index;

	public SectorBuildingStatus SectorBuildingStatus;

	/// <summary>
	///     Position on the minimap
	/// </summary>
	public Vector2 SectorPosition;

	public bool SupplyLineForSector;

	private List<GameNode> _sectorNodes = new();

	public Sector(GameNode[,] map, Vector2I sectorPosition, byte size, byte index) {
		_map = map;
		_size = size;
		SectorPosition = sectorPosition;
		Index = index;
		CameraPosition =
			MapHelpers.GridCoordToGridCenterPos(MapHelpers.CalculateOffset(new Vector2(size - 1, size - 1), index));
		CreateSector();
	}

	private void CreateSector() {
		GenerateMap();
		GenerateNeighbours();
	}

	private void GenerateMap() {
		int centerDist = _size / 2;
		int centerDist2 = centerDist * centerDist;
		for (int i = 0; i < _size; ++i) {
			for (int j = 0; j < _size; ++j) {
				int dist = (i - centerDist) * (i - centerDist) + (j - centerDist) * (j - centerDist);
				if (dist >= centerDist2) {
					continue;
				}
				var offset = MapHelpers.CalculateOffset(i, j, Index);
				var gameNode = new GameNode(offset);
				_map[offset.X, offset.Y] = gameNode;
				_sectorNodes.Add(gameNode);
			}
		}
	}

	public List<GameNode> SectorGameNodes() {
		return [.._sectorNodes];
	}

	private void GenerateNeighbours() {
		var diameter = (_size * 2) - 1;
		for (var i = 0; i < diameter; i++) {
			for (var j = 0; j < diameter; j++) {
				var offset = MapHelpers.CalculateOffset(i, j, Index);
				var item = _map[offset.X, offset.Y];
				if (item is not null) {
					SetupNeighbours(item);
				}
			}
		}
	}

	private void SetupNeighbours(GameNode node) {
		var diameter = (_size * 2) - 1;
		List<Vector2> directions = new() {
			new Vector2(-1, -1),
			new Vector2(-1, 0),
			new Vector2(-1, 1),
			new Vector2(0, -1),
			new Vector2(0, 1),
			new Vector2(1, -1),
			new Vector2(1, 0),
			new Vector2(1, 1)
		};

		foreach (var direction in directions) {
			var newPos = node.Position + direction;
			var offset = MapHelpers.CalculateOffset(0, 0, Index);
			if (newPos.X < offset.X || newPos.Y < offset.Y || newPos.X > offset.X + diameter - 1 ||
			    newPos.Y > offset.Y + diameter - 1) {
				continue;
			}

			var newNeighbour = _map[(int)newPos.X, (int)newPos.Y];
			if (newNeighbour is not null) {
				node.AddNeighbour(newNeighbour, direction.Length());
			}
		}
	}

	public Vector2 CenterPosition() {
		return MapHelpers.CalculateOffset(new Vector2(_size - 1, _size - 1), Index);
	}
}
