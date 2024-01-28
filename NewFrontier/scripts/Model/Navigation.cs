using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public record PriorityNode(GameNode Node, float Priority);

public class Navigation {
	private readonly MapGrid _mapGrid;

	public Navigation(MapGrid mapGrid) {
		_mapGrid = mapGrid;
	}

	public IEnumerable<GameNode> FindPath(GameNode start, GameNode end) {
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

		var path = RetracePath(start, end, cameFrom).ToList();
		return SimplifyPath(path);
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

	private List<GameNode> SimplifyPath(List<GameNode> path) {
		if (path.Count < 3) {
			return path;
		}

		GD.Print("-----PATH");
		GD.Print(string.Join(", ", path.Select(x => x.PositionI)));
		GD.Print("PATH----");

		List<GameNode> simplifiedPath = [path[0]];
		GameNode current = path[0];

		for (int i = 2; i < path.Count; i++) {
			if (!HasClearPath(current, path[i])) {
				// Obstacle encountered, add the last clear point to the simplified path
				simplifiedPath.Add(current);
				current = path[i - 1];
			}
		}

		simplifiedPath.Add(path[^1]); // Include the last point

		return simplifiedPath;
	}

	static List<Vector2I> GetPointsInBetween(Vector2I start, Vector2I end) {
		List<Vector2I> pointsInBetween = new List<Vector2I>();

		int dx = Math.Abs(end.X - start.X);
		int dy = Math.Abs(end.Y - start.Y);
		int sx = (start.X < end.X) ? 1 : -1;
		int sy = (start.Y < end.Y) ? 1 : -1;

		int err = dx - dy;

		int currentX = start.X;
		int currentY = start.Y;

		while (true) {
			pointsInBetween.Add(new Vector2I(currentX, currentY));

			if (currentX == end.X && currentY == end.Y) {
				break; // Reached the end
			}

			int e2 = 2 * err;
			if (e2 > -dy) {
				err -= dy;
				currentX += sx;
			}

			if (e2 < dx) {
				err += dx;
				currentY += sy;
			}
		}

		return pointsInBetween;
	}

	private bool HasClearPath(GameNode startNode, GameNode endNode) {
		var start = startNode.PositionI;
		var end = endNode.PositionI;
		var startGlobalPos = MapHelpers.GridCoordToGridCenterPos(start);
		var endGlobalPos = MapHelpers.GridCoordToGridCenterPos(end);
		var subpath = GetPointsInBetween(new Vector2I((int)startGlobalPos.X, (int)startGlobalPos.Y),
			new Vector2I((int)endGlobalPos.X, (int)endGlobalPos.Y));
		GD.Print($"PATHZ: {string.Join(", ", subpath)}");
		return subpath.Select(point => MapHelpers.PosToGridPoint(point))
			.All(a => !_mapGrid[a.X, a.Y].Blocking);
	}
}
