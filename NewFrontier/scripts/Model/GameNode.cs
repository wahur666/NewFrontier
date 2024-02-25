using System.Collections.Generic;
using Godot;
using NewFrontier.scripts.Entities;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public class GameNode(Vector2 pos) {
	// TODO: remove this in favour of the attributes
	public int ActiveAttribute;

	// TODO: replace this with a proper enums
	public bool Blocking;
	public BuildingNode2D Building;
	public bool HasWormhole;
	public Dictionary<GameNode, float> Neighbours = new();
	public bool Occupied;
	public UnitNode2D PreOccupied = null;
	public int PassiveAttribute;
	public Vector2 Position = pos;
	public Vector2I PositionI { get => new((int)pos.X, (int)pos.Y); }
	public int StaticAttribute;

	public GameNode WormholeNode;

	public bool FreeNode() {
		return !Blocking && PreOccupied is null && !Occupied && !HasWormhole;
	}

	public byte SectorIndex => MapHelpers.GetSectorIndexFromOffset(Position);

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

	public void SetBuilding(BuildingNode2D buildingNode2D, bool attach) {
		Building = buildingNode2D;
		if (attach) {
			buildingNode2D.Position = MapHelpers.GridCoordToGridCenterPos(Position);
		}
	}
}
