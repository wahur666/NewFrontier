using System;
using System.Collections.Generic;
using System.IO;
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

		var path = RetracePath(start, end, cameFrom);
		return path;
		var optimized = OptimisePath(path);
		return optimized;
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

	private IEnumerable<GameNode> OptimisePath(IEnumerable<GameNode> path) {
		var optimisePath = path.ToList();
		if (optimisePath.Count < 2) {
			return optimisePath;
		}
		var start = 0;
		var current = 2;
		List<GameNode> nPath  = [optimisePath[start]];
		var i = 0;
		const int maxIterations = 1000;
		for (; i < maxIterations && current < optimisePath.Count; i++) {
			var p1 = optimisePath[start].Position;
			var p2 = optimisePath[current].Position;
			if (optimisePath[start].HasWormhole || optimisePath[current].HasWormhole) {
				nPath.Add(optimisePath[current - 1]);
				start = current - 1;
				current = start + 2;
				continue;
			}
			var pixels = GetPixelsOnLine(p1, p2);
			if (pixels.All(p => _mapGrid[(int)p.X, (int)p.Y] is not null && !_mapGrid[(int)p.X, (int)p.Y].Blocking)) {
				current += 1;
			} else {
				nPath.Add(optimisePath[current - 1]);
				start = current - 1;
				current = start + 2;
			}
		}
		if (i == maxIterations) {
			GD.Print($"Path optimization failed {optimisePath}");
			return optimisePath;
		}

		nPath.Add(optimisePath[^1]);
		GD.Print($"Path optimized {optimisePath.Count} -> ${nPath.Count}");
		return nPath;
	}
    
    private List<Vector2> GetPixelsOnLine(Vector2 p1, Vector2 p2) {
            List<Vector2> pixels = [];
    
            // Calculate differences and directions
            var dx = Math.Abs(p2.X - p1.X);
            var dy = Math.Abs(p2.Y - p1.Y);
            var sx = (p1.X < p2.X) ? 1 : -1;
            var sy = (p1.Y < p2.Y) ? 1 : -1;
            var err = dx - dy;
    
            // Initial pixel
            var x = p1.X;
            var y = p1.Y;
    
            while (true) {
                // Add current pixel
                pixels.Add(new Vector2(x, y));
    
                // Check if end point is reached
                if (Math.Abs(x - p2.X) < 0.01 && Math.Abs(y - p2.Y) < 0.01) {
                    break;
                }
    
                // Calculate next pixel
                var e2 = 2 * err;
                if (e2 > -dy) {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx) {
                    err += dx;
                    y += sy;
                }
            }
    
            return pixels;
        }
}
