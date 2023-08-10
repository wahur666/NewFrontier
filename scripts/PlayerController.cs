using Godot;
using System;
using System.Collections.Generic;

namespace NewFrontier.scripts;

public partial class PlayerController : Node {

	public int MaxCrew = 2500;
	public int MaxOre = 5500;
	public int MaxGas = 4500;
	public int MaxSupply = 0;

	public int CurrentCrew = 0;
	[Export]public int CurrentOre = 0;
	[Export]public int CurrentGas = 0;
	public int CurrentSupply = 0;

	private List<BuildingNode2D> _buildings = new();
	private List<UnitNode2D> _units = new();
	private BuildingNode2D _buildingShade = null;

	private PackedScene _base1;
	private PackedScene _base2;
	private PackedScene _base3;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_base1 = GD.Load<PackedScene>("res://scenes/base_1.tscn");
		_base2 = GD.Load<PackedScene>("res://scenes/base_2.tscn");
		_base3 = GD.Load<PackedScene>("res://scenes/base_3.tscn");
		GD.Print("Player controller loaded to scene");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("RMB")) {
			FreeBuildingShade();
		}
	}

	public int AvailableOreStorage() => MaxOre - CurrentOre;
	public int AvailableGasStorage() => MaxGas - CurrentGas;
	public int AvailableCrewStorage() => MaxCrew - CurrentCrew;

	private void FreeBuildingShade() {
		_buildingShade?.QueueFree();
		_buildingShade = null;
	}

	public void SetBuildingShadeVisibility(bool visible) {
		if (_buildingShade is not null) {
			_buildingShade.Visible = visible;
		}
	}
	
	public void CreateBuilding1() {
		if (_buildingShade?.Wide == 1) {
			return;
		}
		FreeBuildingShade();
		_buildingShade = _base1.Instantiate<BuildingNode2D>();
		_buildingShade.SetPlayer(this);
		AddChild(_buildingShade);
	}
	
	public void CreateBuilding2() {
		if (_buildingShade?.Wide == 2) {
			return;
		}
		FreeBuildingShade();
		_buildingShade = _base2.Instantiate<BuildingNode2D>();
		_buildingShade.SetPlayer(this);
		AddChild(_buildingShade);
		
	}
	
	public void CreateBuilding3() {
		if (_buildingShade?.Wide == 3) {
			return;
		}
		FreeBuildingShade();
		_buildingShade = _base3.Instantiate<BuildingNode2D>();
		_buildingShade.SetPlayer(this);
		AddChild(_buildingShade);
	}

	public void BuildBuilding(BuildingNode2D buildingNode2D, Planet planet, int[] place) {
		var building = planet.BuildBuilding(buildingNode2D, place);
		if (building is not null) {
			_buildings.Add(building);
		}
	}
}
