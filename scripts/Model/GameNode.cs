using Godot;
using System.Collections.Generic;

namespace NewFrontier.scripts.Model;

public class GameNode {
	public Vector2 Position;
	public Dictionary<GameNode, float> Neighbours;
	public bool HasWormhole;
	public bool Blocking = false;
	public bool Occupied = false;

	public GameNode(Vector2I pos) {
		this.Position = pos;
		this.Neighbours = new Dictionary<GameNode, float>();
	}

	public bool Equals(GameNode node) {
		return this.Position.Equals(node.Position);
	}

	public void AddNeighbour(GameNode node, float weight) {
		this.Neighbours.Add(node, weight);
	}

	public float Distance(GameNode otherNode) {
		return this.Position.DistanceTo(otherNode.Position);
	}

	public float Weight(GameNode otherNode) {
		const float notANeighbour = 1f;
		return this.Neighbours.GetValueOrDefault(otherNode, notANeighbour);
	}
}
