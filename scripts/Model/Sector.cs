using System;
using System.Collections.Generic;
using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public class Sector {
	/// <summary>
	/// GameMap object
	/// </summary>
	private GameNode[,] _map;

	/// <summary>
	/// Size of the Sector max 128
	/// </summary>
	private byte _size;

	/// <summary>
	/// Position on the minimap
	/// </summary>
	private Vector2 _position;

	/// <summary>
	/// Index if the sector, to allocate the memory, 0-15
	/// </summary>
	private byte _index;

	public Vector2 CameraPosition;

	public Sector(GameNode[,] map, Vector2I position, byte size, byte index) {
		_map = map;
		_size = size;
		_position = position;
		_index = index;
		CreateSector();
	}

	private void CreateSector() {
		GenerateMap();
		GenerateNeighbours();
	}

	private void GenerateMap() {
		var radius = _size - 1;
		var center = new Vector2(_size - 0.5f, _size - 0.5f);
		var diameter = _size * 2 - 1;
		for (int i = 0; i < diameter + 1; i++) {
			for (int j = 0; j < diameter + 1; j++) {
				if (AreaHelper.IsVector2InsideCircle(new(i + 0.5f, j + 0.5f), center, radius + 0.5f)) {
					var offset = MapHelpers.CalculateOffset(i, j, _index);
					_map[offset.X, offset.Y] = new(offset);
				}
			}
		}
	}
	
	private void GenerateNeighbours() {
		var diameter = _size * 2 - 1;
		for (var i = 0; i < diameter; i++) {
			for (var j = 0; j < diameter; j++) {
				var item = _map[i, j];
				if (item is not null) {
					this.setupNeighbours(item);
				}
			}
		}
	}
	
	private void setupNeighbours(GameNode node) {
		var diameter = _size * 2 - 1;
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
			if (newPos.X < 0 || newPos.Y < 0 || newPos.X > diameter - 1 || newPos.Y > diameter - 1) {
				continue;
			}

			var newNeighbour = _map[(int)newPos.X, (int)newPos.Y];
			if (newNeighbour is not null) {
				node.AddNeighbour(newNeighbour, direction.Length());
			}
		}

		// _wormholes.Where(wormhole => wormhole.IsConnected(node))
		// 	.ToList()
		// 	.ForEach(wormhole => node.AddNeighbour(wormhole.GetOtherNode(node), wormhole.Distance));
	}
}
