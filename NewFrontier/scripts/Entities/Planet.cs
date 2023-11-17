using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace NewFrontier.scripts.Entities;

internal enum SlotStatus {
	Free,
	Occupied
}

public partial class Planet : Node2D {
	public static readonly int Radius = 150;

	private readonly List<BuildingNode2D> _buildings = new();
	private Node _buildingsContainer;
	private PlanetBuildingScheme _planetBuildingScheme;
	private readonly SlotStatus[] _slots = new SlotStatus[12];

	public override void _Ready() {
		_buildingsContainer = GetNode<Node>("BuildingsContainer");
		_planetBuildingScheme = GetNode<PlanetBuildingScheme>("PlanetBuildingScheme");
		_planetBuildingScheme.Radius = Radius;
	}

	public override void _Process(double delta) {
	}
	
	public bool PointNearToRing(Vector2 mousePos) {
		return Math.Abs(mousePos.DistanceTo(GlobalPosition) - Radius) < 20;
	}

	public BuildingNode2D BuildBuilding(BuildingNode2D building) {
		var slots = building.Place;
		if (slots.Any(slot => _slots[slot] == SlotStatus.Occupied)) {
			return null;
		}

		foreach (var slot in slots) {
			_slots[slot] = SlotStatus.Occupied;
		}

		var newBuilding = building.Duplicate() as BuildingNode2D;
		newBuilding.BuildingShade = false;
		_buildings.Add(newBuilding);
		_buildingsContainer.AddChild(newBuilding);
		return newBuilding;
	}
}
