using System.Collections.Generic;
using Godot;

namespace NewFrontier.scripts.Model;

public class GameNode {
	public int ActiveAttribute;
	public bool Blocking;

	// TODO: remove this in favour of the attributes
	public bool HasWormhole;
	public Dictionary<GameNode, float> Neighbours;
	public bool Occupied;

	// TODO: replace this with a proper enums
	public int PassiveAttribute;
	public Vector2 Position;
	public int StaticAttribute;

	public GameNode WormholeNode;

	public GameNode(Vector2 pos) {
		Position = pos;
		Neighbours = new Dictionary<GameNode, float>();
	}

	public bool Equals(GameNode node) {
		return Position.Equals(node.Position);
	}

	public void AddNeighbour(GameNode node, float weight) {
		Neighbours.Add(node, weight);
	}

	public void AddNeighbourTwoWays(GameNode node, float weight) {
		Neighbours.Add(node, weight);
		node.Neighbours.Add(this, weight);
	}

	public float Distance(GameNode otherNode) {
		return Position.DistanceTo(otherNode.Position);
	}

	public float Weight(GameNode otherNode) {
		const float notANeighbour = 1f;
		return Neighbours.GetValueOrDefault(otherNode, notANeighbour);
	}

	public void SetWormhole(GameNode wormholeNode) {
		WormholeNode = wormholeNode;
		HasWormhole = true;
	}
}
