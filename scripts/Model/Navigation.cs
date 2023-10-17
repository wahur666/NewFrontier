using System;
using System.Collections.Generic;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public record PriorityNode(GameNode Node, float Priority);

public class PriorityNodeComparer : IComparer<PriorityNode> {
	public int Compare(PriorityNode x, PriorityNode y) => x.Priority.CompareTo(y.Priority);
}

public class Navigation {
	private MapGrid _mapGrid;

	public Navigation(MapGrid mapGrid) {
		_mapGrid = mapGrid;
	}

	public IEnumerable<GameNode> FindPath(GameNode start, GameNode end) {
		if (_mapGrid.PassiveGridLayer[(int)start.Position.X, (int)start.Position.Y] is null ||
		    _mapGrid.PassiveGridLayer[(int)end.Position.X, (int)end.Position.Y] is null) {
			return Array.Empty<GameNode>();
		}

		var frontier = new Heap<PriorityNode>((a, b) => a.Priority.CompareTo(b.Priority));
		frontier.Add(new PriorityNode(start, 0));
		var cameFrom = new Dictionary<GameNode, GameNode?>();
		var costSoFar = new Dictionary<GameNode, int>();
		cameFrom[start] = null;
		costSoFar[start] = 0;
		while (frontier.Count != 0) {
			var current = frontier.PopTop();
			if (current.Node == end) {
				break;
			}

			foreach (var next in _mapGrid.NodeNeighbours(current.Node)) {
				var newCost = costSoFar[current.Node] + current.Node.Weight(next);
				if (!cameFrom.ContainsKey(next) || newCost < costSoFar[next]) {
					costSoFar.Add(next, newCost);

					var priority = newCost + end.Distance(next);

					frontier.Add(new PriorityNode(next, priority));
					cameFrom.Add(next, current.Node);
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
