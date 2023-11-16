using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Entities;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Controllers;

public partial class PlayerController : Node {
	private PackedScene _base1;
	private PackedScene _base2;
	private PackedScene _base3;

	private readonly List<BuildingNode2D> _buildings = new();
	private BuildingNode2D _buildingShade;
	private CameraController _camera;
	private PackedScene _fabricator;
	private PackedScene _harvester;

	private MapGrid _mapGrid;
	private bool _overGui;
	private bool _overSectormap;
	private List<UnitNode2D> _units = new();
	private PlayerStats _playerStats = new();

	public byte CurrentSector;

	public LeftControls LeftControls;

	public UiController UiController;

	private Vector2 end;
	private Vector2 endV;
	private Vector2 mousePosGlobal;
	private Vector2 start;
	private Vector2 startV;
	private Vector2 mousePosition;
	private bool _dragging;


	public bool OverGui {
		get => _overGui;
		set {
			GD.Print("setting over gui ", value);
			_overGui = value;
		}
	}

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
		_camera.PointSelected += SelectUnitNearPoint;
		_camera.PlayerControllerInstance = this;
		_mapGrid = GetNode<MapGrid>("../../MapGrid");
		UiController = GetNode<UiController>("../../Ui");
		UiController.Init(this, _mapGrid);
		CreateStartingUnits();
		_camera.CenterOnGridPosition(new Vector2(12, 17));
		SetupUiControllerHandlers();
	}

	private void SetupUiControllerHandlers() {
		UiController.SectorPanel.MouseEntered += () => {
			GD.Print("Mouse entered sector element");
			_overSectormap = true;
		};
		UiController.SectorPanel.MouseExited += () => {
			GD.Print("Mouse exited sector element");
			_overSectormap = false;
		};
	}

	private void CreateStartingUnits() {
		_units = new List<UnitNode2D>();
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
		if (Input.IsActionJustPressed("LMB")) {
			var sectorPanelGlobalPosition = UiController.SectorPanel.GlobalPosition;
			var sectorPanelSize = UiController.SectorPanel.Size;
			var overSectorMap = AreaHelper.InRect(mousePosition, sectorPanelGlobalPosition,
				sectorPanelGlobalPosition + sectorPanelSize);
			if (overSectorMap) {
				CheckSectorMapClick();
			} else {
				start = _camera.GetGlobalMousePosition();
				startV = mousePosition;
				_dragging = true;
				_camera.EnableEdgePanning = false;
			}
		} else if (Input.IsActionJustPressed("RMB")) {
			if (BuildingMode) {
				FreeBuildingShade();
				BuildingMode = false;
			} else {
				start = _camera.GetGlobalMousePosition();
				startV = mousePosition;
				_dragging = false;
				_camera.EnableEdgePanning = true;
				DrawArea(false);
				MoveToPoint(start);
			}
		} else if (Input.IsActionJustReleased("LMB")) {
			end = _camera.GetGlobalMousePosition();
			endV = mousePosition;
			_dragging = false;
			_camera.EnableEdgePanning = true;
			DrawArea(false);
			if (start.DistanceTo(end) > 10) {
				SelectUnitsInArea(start, end);
			} else if (!OverUiElement(start)) {
				SelectUnitNearPoint(start);
			}
		}

		if (_dragging) {
			end = _camera.GetGlobalMousePosition();
			endV = mousePosition;
			DrawArea();
		}
	}

	private void CheckSectorMapClick() {
		var a = GetViewport().GetMousePosition();
		var b = UiController.SectorPanel.GlobalPosition;
		var z = _mapGrid.Sectors.Where(x => x.Discovered).ToList()
			.Find(x => Math.Abs((x.SectorPosition + b - a).Length()) < 10);
		if (z is null) {
			return;
		}

		_camera.Position =
			MapHelpers.GridCoordToGridCenterPos(MapHelpers.CalculateOffset(new Vector2(15, 15), z.Index));
		CurrentSector = z.Index;
	}

	public void DrawArea(bool s = true) {
		var panel = UiController.SelectionPanel;
		panel.Size = new Vector2(Math.Abs(startV.X - endV.X), Math.Abs(startV.Y - endV.Y));
		var pos = Vector2.Zero;
		pos.X = Math.Min(startV.X, endV.X);
		pos.Y = Math.Min(startV.Y, endV.Y);
		panel.Position = pos;
		panel.Size *= s ? 1 : 0;
	}


	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouse mouse) {
			mousePosition = mouse.Position;
			mousePosGlobal = _camera.GetGlobalMousePosition();
		}
	}


	public int AvailableOreStorage() {
		return _playerStats.MaxOre - _playerStats.CurrentOre;
	}

	public int AvailableGasStorage() {
		return _playerStats.MaxGas - _playerStats.CurrentGas;
	}

	public int AvailableCrewStorage() {
		return _playerStats.MaxCrew - _playerStats.CurrentCrew;
	}

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
		_buildingShade.Init(this);
		AddChild(_buildingShade);
	}

	public void CreateBuilding2() {
		BuildingMode = true;
		if (_buildingShade?.Wide == 2) {
			return;
		}

		FreeBuildingShade();
		_buildingShade = _base2.Instantiate<BuildingNode2D>();
		_buildingShade.Init(this);
		AddChild(_buildingShade);
	}

	public void CreateBuilding3() {
		BuildingMode = true;
		if (_buildingShade?.Wide == 3) {
			return;
		}

		FreeBuildingShade();
		_buildingShade = _base3.Instantiate<BuildingNode2D>();
		_buildingShade.Init(this);
		AddChild(_buildingShade);
	}

	public void BuildBuilding(BuildingNode2D buildingNode2D, Planet planet, int[] place) {
		var building = planet.BuildBuilding(buildingNode2D, place);
		if (building is not null) {
			_buildings.Add(building);
		}
	}

	private void SelectUnitsInArea(Vector2 start, Vector2 end) {
		if (OverGui) {
			return;
		}

		GD.Print("this is running SelectUnitsInArea");
		BuildingMode = false;
		foreach (var unit in _units) {
			unit.Selected = AreaHelper.InRect(unit.Position, start, end);
		}

		UpdateUi();
	}

	private void SelectUnitNearPoint(Vector2 point) {
		if (OverGui) {
			return;
		}

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

	private void MoveToPoint(Vector2 targetVector2) {
		_units.Where(x => x.Selected)
			.ToList()
			.ForEach(unitNode2D => {
					var start = MapHelpers.PosToGrid(unitNode2D.Position);
					var end = MapHelpers.PosToGrid(targetVector2);
					var path = _mapGrid.Navigation
						.FindPath(start, end)
						.ToList();
					unitNode2D.SetNavigation(path);
				}
			);
	}

	public void IncreaseOre(int amount) => _playerStats.CurrentOre += amount;
	public void IncreaseGas(int amount) => _playerStats.CurrentGas += amount;
	public void IncreaseCrew(int amount) => _playerStats.CurrentCrew += amount;

	private bool OverUiElement(Vector2 position) {
		return UiController.OverUiElement(position);
	}
}
