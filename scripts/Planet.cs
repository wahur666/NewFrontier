using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewFrontier.scripts;

enum SlotStatus {
	Free,
	Occupied,
}

public partial class Planet : Node2D {
	public int Radius = 83;

	private List<BuildingNode2D> _buildings = new();
	private SlotStatus[] _slots = new SlotStatus[12];
	private Node _buildingsContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_buildingsContainer = GetNode<Node>("BuildingsContainer");
		GD.Print("slots: " + _slots);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public BuildingNode2D BuildBuilding(BuildingNode2D building, int[] slots) {
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
