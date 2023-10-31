using System;
using System.Collections.Generic;
using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public record PriorityNode(GameNode Node, float Priority);

public class Navigation {
	private MapGrid _mapGrid;

	public Navigation(MapGrid mapGrid) {
		_mapGrid = mapGrid;
	}

	public IEnumerable<GameNode> FindPath(Vector2 startVector2, Vector2 endVector2) {
		var start = _mapGrid.PassiveGridLayer[(int)startVector2.X, (int)startVector2.Y];
		var end = _mapGrid.PassiveGridLayer[(int)endVector2.X, (int)endVector2.Y];
		return FindPath(start, end);
	}
	
	public IEnumerable<GameNode> FindPath(GameNode start, GameNode end) {
		if (start is null || end is null || end.Blocking) {
			return Array.Empty<GameNode>();
		}
		
		var frontier = new Heap<PriorityNode>((a, b) => b.Priority.CompareTo(a.Priority));
		frontier.Add(new PriorityNode(start, 0));
		var cameFrom = new Dictionary<GameNode, GameNode>();
		var costSoFar = new Dictionary<GameNode, float>();
		cameFrom[start] = null;
		costSoFar[start] = 0;
		while (frontier.Count != 0) {
			var current = frontier.PopTop();
			if (current.Node == end) {
				break;
			}

			foreach (var next in _mapGrid.NodeNeighbours(current.Node)) {
				if (next.Blocking) {
					continue;
				}
				var newCost = costSoFar[current.Node] + current.Node.Weight(next);
				if (!cameFrom.ContainsKey(next) || newCost < costSoFar[next]) {
					costSoFar[next] = newCost;
					var priority = newCost + end.Distance(next);

					frontier.Add(new PriorityNode(next, priority));
					cameFrom[next] = current.Node;
				}
			}
		}

		return this.RetracePath(start, end, cameFrom);
	}

	private IEnumerable<GameNode> RetracePath(GameNode start, GameNode end, Dictionary<GameNode, GameNode> cameFrom) {
		var current = end;
		var path = new List<GameNode>();
		while (current != start) {
			path.Add(current);
			current = cameFrom[current];
		}

		path.Add(start);
		path.Reverse();
		return path;
	}
}
