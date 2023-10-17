using Godot;
using System.Collections.Generic;

namespace NewFrontier.scripts.Model;

public class GameNode {
	public Vector2 Position;
	public Dictionary<GameNode, int> Neighbours;
	public bool HasWormhole;

	GameNode(Vector2I pos) {
		this.Position = pos;
		this.Neighbours = new Dictionary<GameNode, int>();
	}

	public bool Equals(GameNode node) {
		return this.Position.Equals(node.Position);
	}

	public void AddNeighbour(GameNode node, int weight) {
		this.Neighbours.Add(node, weight);
	}

	public float Distance(GameNode otherNode) {
		return this.Position.DistanceTo(otherNode.Position);
	}

	public int Weight(GameNode otherNode) {
		const int notANeighbour = 1;
		return this.Neighbours.GetValueOrDefault(otherNode, notANeighbour);
	}
}
