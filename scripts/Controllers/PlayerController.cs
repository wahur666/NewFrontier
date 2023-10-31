using Godot;
using NewFrontier.scripts.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using NewFrontier.scripts.Entities;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.UI;


namespace NewFrontier.scripts.Controllers;

public partial class PlayerController : Node {
	public int MaxCrew = 2500;
	public int MaxOre = 5500;
	public int MaxGas = 4500;
	public int MaxSupply = 0;

	public int CurrentCrew = 0;
	[Export] public int CurrentOre = 0;
	[Export] public int CurrentGas = 0;
	public int CurrentSupply = 0;

	public LeftControls LeftControls;

	private MapGrid _mapGrid;
	private CameraController _camera;

	private List<BuildingNode2D> _buildings = new();
	private List<UnitNode2D> _units = new();
	private BuildingNode2D _buildingShade = null;
	public UiController UiController;

	public bool OverGui {
		get => _overGui;
		set {
			GD.Print("setting over gui ", value);
			_overGui = value;
		}
	}

	private PackedScene _base1;
	private PackedScene _base2;
	private PackedScene _base3;
	private PackedScene _harvester;
	private PackedScene _fabricator;
	private bool _overGui;

	public bool BuildingMode {
		get;
		private set;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_base1 = GD.Load<PackedScene>("res://scenes/base_1.tscn");
		_base2 = GD.Load<PackedScene>("res://scenes/base_2.tscn");
		_base3 = GD.Load<PackedScene>("res://scenes/base_3.tscn");
		_harvester = GD.Load<PackedScene>("res://scenes/harvester.tscn");
		_fabricator = GD.Load<PackedScene>("res://scenes/fabricator.tscn");
		_camera = GetNode<CameraController>("../../Camera2D");
		_camera.AreaSelected += SelectUnitsInArea;
		_camera.PointSelected += SelectUnitNearPoint;
		_camera.MoveToPoint += MoveToPoint;
		_camera.PlayerControllerInstance = this;
		_mapGrid = GetNode<MapGrid>("../../MapGrid");
		UiController = GetNode<UiController>("../../Ui");
		UiController.Init(this);
		CreateStartingUnits();
	}


	private void CreateStartingUnits() {
		_units = new();
		var harvester = _harvester.Instantiate<Harvester>();
		harvester.Init(new Vector2(10, 10), this, UiController);
		_units.Add(harvester);
		var fabricator = _fabricator.Instantiate<Fabricator>();
		fabricator.Init(new Vector2(12, 17), this, UiController);
		_units.Add(fabricator);
		_units.ForEach(e => AddChild(e));
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
		_buildingShade.ZIndex = 10;
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
		_buildingShade.ZIndex = 10;
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
		_buildingShade.ZIndex = 10;
		AddChild(_buildingShade);
	}

	public void BuildBuilding(BuildingNode2D buildingNode2D, Planet planet, int[] place) {
		var building = planet.BuildBuilding(buildingNode2D, place);
		if (building is not null) {
			_buildings.Add(building);
		}
	}

	private void SelectUnitsInArea(Vector2 start, Vector2 end) {
		if (OverGui) return;
		GD.Print("this is running SelectUnitsInArea");
		BuildingMode = false;
		foreach (var unit in _units) {
			unit.Selected = AreaHelper.InRect(unit.Position, start, end);
		}

		UpdateUi();
	}

	private void SelectUnitNearPoint(Vector2 point) {
		if (OverGui) return;
		GD.Print("this is running SelectUnitNearPoint");
		if (BuildingMode) {
			return;
		}

		_units.ForEach(x => x.Selected = false);
		var unitNode2D = _units.Find(x => x.InsideSelectionRect(point));
		if (unitNode2D is not null) {
			unitNode2D.Selected = true;
		}

		UpdateUi();
	}

	private void UpdateUi() {
		var units = _units.Where(x => x.Selected).ToList();
		LeftControls.SetBuildingContainerVisibility(units.Count == 1 && units[0] is Fabricator);
		LeftControls.CalculateSelectedUnits(_units.Where(x => x.Selected).ToList());
	}

	public void SelectUnit(UnitNode2D unit) {
		GD.Print("this is running SelectUnit");
		_units.ForEach(x => x.Selected = x == unit);
		UpdateUi();
	}

	private void MoveToPoint(Vector2 point) {
		// _units.Where(x => x.Selected).ToList().ForEach(node2D => node2D.TargetDestination = point);
		_units.Where(x => x.Selected).ToList().ForEach(a => {
			var currGrid = MapHelpers.PosToGrid(a.Position);
			var targetGrid = MapHelpers.PosToGrid(point);
			GD.Print("Moving from", currGrid, targetGrid);
			var path = _mapGrid.Navigation.FindPath(currGrid, targetGrid);
			var gameNodes = path.ToList();
			var ab = gameNodes.Select(x => x.Position).Select(x => $"({x.X}, {x.Y})");
			GD.Print("Path", ab.ToArray().Stringify());
			a.SetNavigation(gameNodes.Select(x => MapHelpers.GridCoordToGridCenterPos(x.Position)).ToList());
		});
	}
}
