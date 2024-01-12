using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.Entities;

public partial class Planet : Node2D {
	public const int Radius = 150;

	public List<BuildingNode2D> Buildings = [];
	private readonly bool[] _slotOccupiedStatus = new Boolean[12];
	private Node _buildingsContainer;
	private PlanetBuildingScheme _planetBuildingScheme;
	public PlanetStats PlanetStats;
	public string PlanetName = "Kappa 2";
	private Timer _resourceTimer;

	private Area2D _selectionArea;

	public bool MouseOver;
	
	[Export] public PlanetType PlanetType = PlanetType.Earth;

	public override void _Ready() {
		_buildingsContainer = GetNode<Node>("BuildingsContainer");
		_planetBuildingScheme = GetNode<PlanetBuildingScheme>("PlanetBuildingScheme");
		PlanetStats = GetNode<PlanetStats>("Stats");
		_selectionArea = GetNode<Area2D>("SelectionArea");
		_selectionArea.MouseEntered += () => MouseOver = true;
		_selectionArea.MouseExited += () => MouseOver = false;
		_planetBuildingScheme.Radius = Radius;
		_resourceTimer = GetNode<Timer>("ResourceTimer");
		_resourceTimer.Timeout += ResourceTimerOnTimeout;
	}

	private void ResourceTimerOnTimeout() {
		PlanetStats.IncreaseResources();
	}

	public bool PointNearToRing(Vector2 mousePos) {
		return Math.Abs(mousePos.DistanceTo(GlobalPosition) - Radius) < 20;
	}

	public BuildingNode2D BuildBuilding(IBuildable building) {
		var slots = building.Place;
		if (slots.Any(slot => _slotOccupiedStatus[slot])) {
			return null;
		}

		foreach (var slot in slots) {
			_slotOccupiedStatus[slot] = true;
		}

		var newBuilding = building.Instance.Duplicate() as BuildingNode2D;
		newBuilding.BuildingShade = false;
		Buildings.Add(newBuilding);
		_buildingsContainer.AddChild(newBuilding);
		if (newBuilding is Refinery refinery) {
			refinery.StartTimers();
		}
		return newBuilding;
	}
}
