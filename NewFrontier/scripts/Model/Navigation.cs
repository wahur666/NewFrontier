using System;
using System.Collections.Generic;
using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public record PriorityNode(GameNode Node, float Priority);

public class Navigation {
	private readonly MapGrid _mapGrid;

	public Navigation(MapGrid mapGrid) {
		_mapGrid = mapGrid;
	}

	public IEnumerable<GameNode> FindPath(Vector2I startVector2, Vector2I endVector2) {
		var start = _mapGrid.GridLayer[startVector2.X, startVector2.Y];
		var end = _mapGrid.GridLayer[endVector2.X, endVector2.Y];
		return FindPath(start, end);
	}

	private IEnumerable<GameNode> FindPath(GameNode start, GameNode end) {
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

			foreach (var next in current.Node.Neighbours.Keys) {
				if (next.Blocking) {
					continue;
				}

				var newCost = costSoFar[current.Node] + current.Node.Weight(next);
				if (cameFrom.ContainsKey(next) && !(newCost < costSoFar[next])) {
					continue;
				}

				costSoFar[next] = newCost;
				var priority = newCost + end.Distance(next);

				frontier.Add(new PriorityNode(next, priority));
				cameFrom[next] = current.Node;
			}
		}

		return RetracePath(start, end, cameFrom);
	}

	private static IEnumerable<GameNode> RetracePath(GameNode start, GameNode end,
		IReadOnlyDictionary<GameNode, GameNode> cameFrom) {
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
