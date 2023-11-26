using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Entities;

public partial class Planet : Node2D {
	public static readonly int Radius = 150;

	private readonly List<BuildingNode2D> _buildings = new();
	private Node _buildingsContainer;
	private PlanetBuildingScheme _planetBuildingScheme;
	private readonly bool[] _slotOccupiedStatus = new Boolean[12];

	[Export] public PlanetType PlanetType = PlanetType.Earth;

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
		if (slots.Any(slot => _slotOccupiedStatus[slot])) {
			return null;
		}

		foreach (var slot in slots) {
			_slotOccupiedStatus[slot] = true;
		}

		var newBuilding = building.Duplicate() as BuildingNode2D;
		newBuilding.BuildingShade = false;
		_buildings.Add(newBuilding);
		_buildingsContainer.AddChild(newBuilding);
		return newBuilding;
	}
}
