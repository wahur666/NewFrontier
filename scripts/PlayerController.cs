using Godot;
using NewFrontier.scripts.helpers;
using System;
using System.Collections.Generic;
using System.Linq;


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

	public LeftControls LeftControls;


	private CameraController _camera;
	
	private List<BuildingNode2D> _buildings = new();
	private List<UnitNode2D> _units = new();
	private BuildingNode2D _buildingShade = null;

	private PackedScene _base1;
	private PackedScene _base2;
	private PackedScene _base3;
	
	public bool BuildingMode {
		get;
		private set;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_base1 = GD.Load<PackedScene>("res://scenes/base_1.tscn");
		_base2 = GD.Load<PackedScene>("res://scenes/base_2.tscn");
		_base3 = GD.Load<PackedScene>("res://scenes/base_3.tscn");
		GD.Print("Player controller loaded to scene");

		_camera = GetNode<CameraController>("../../Camera2D");
		_camera.AreaSelected += SelectUnitsInArea;
		_camera.PointSelected += SelectUnitNearPoint;
		_camera.PlayerControllerInstance = this;
		_units = GetNode("../../Units").GetChildren().Select(x => x as UnitNode2D).ToList();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("RMB")) {
			FreeBuildingShade();
			BuildingMode = false;
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
		BuildingMode = true;
		if (_buildingShade?.Wide == 1) {
			return;
		}
		FreeBuildingShade();
		_buildingShade = _base1.Instantiate<BuildingNode2D>();
		_buildingShade.SetPlayer(this);
		AddChild(_buildingShade);
	}
	
	public void CreateBuilding2() {
		BuildingMode = true;
		if (_buildingShade?.Wide == 2) {
			return;
		}
		FreeBuildingShade();
		_buildingShade = _base2.Instantiate<BuildingNode2D>();
		_buildingShade.SetPlayer(this);
		AddChild(_buildingShade);
		
	}
	
	public void CreateBuilding3() {
		BuildingMode = true;
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

	public void SelectUnitsInArea(Vector2 start, Vector2 end) {
		BuildingMode = false;
		foreach (var unit in _units) {
			unit.Selected = AreaHelper.InRect(unit.Position, start, end);
		}
		UpdateUI();
	}

	public void SelectUnitNearPoint(Vector2 point) {
		if (BuildingMode) {
			return;
		}
		_units.ForEach(x => x.Selected = false);
		// TODO refactor to only include unit if its inside its selected area
		var unitNode2D = _units.Find(x => x.Position.DistanceTo(point) < 30); 
		if (unitNode2D is not null) {
			unitNode2D.Selected = true;
		}
		UpdateUI();
	}

	private void UpdateUI() {
		LeftControls.SetBuildingContainerVisibility(_units.Where(x => x.Selected && x is Fabricator).Count() == 1);
	}

}
