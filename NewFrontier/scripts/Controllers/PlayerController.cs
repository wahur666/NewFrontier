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
	private List<UnitNode2D> _units = new();
	private List<UnitNode2D> _selectedUnits = new();
	private PlayerStats _playerStats = new();

	public byte CurrentSector;

	public LeftControls LeftControls;
	private UiController _uiController;

	#region Dragging variables

	private Vector2 _dragEnd;
	private Vector2 _dragEndV;
	private Vector2 _mousePosGlobal;
	private Vector2 _dragStart;
	private Vector2 _dragStartV;
	private Vector2 _mousePosition;
	private bool _dragging;

	#endregion

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
		_camera.PlayerControllerInstance = this;
		_mapGrid = GetNode<MapGrid>("../../MapGrid");
		_uiController = GetNode<UiController>("../../Ui");
		_uiController.Init(this, _mapGrid);
		CreateStartingUnits();
		_camera.CenterOnGridPosition(new Vector2(12, 17));
	}

	private void CreateStartingUnits() {
		_units = new List<UnitNode2D>();
		var harvester = _harvester.Instantiate<Harvester>();
		harvester.Init(new Vector2(10, 10), this, _uiController);
		_units.Add(harvester);
		var fabricator = _fabricator.Instantiate<Fabricator>();
		fabricator.Init(new Vector2(12, 17), this, _uiController);
		_units.Add(fabricator);
		_units.ForEach(e => AddChild(e));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("LMB")) {
			_dragStart = _camera.GetGlobalMousePosition();
			_dragStartV = _mousePosition;
			if (_buildingShade?.Planet is not null) {
				BuildBuilding(_buildingShade);
			} else if (_uiController.MouseOverSectorMap(_mousePosition)) {
				CheckSectorMapClick();
			} else {
				_dragging = true;
				_camera.EnableEdgePanning = false;
			}
		} else if (Input.IsActionJustPressed("RMB")) {
			if (BuildingMode) {
				FreeBuildingShade();
				BuildingMode = false;
			} else {
				_dragStart = _camera.GetGlobalMousePosition();
				_dragStartV = _mousePosition;
				_dragging = false;
				_camera.EnableEdgePanning = true;
				DrawArea(false);
				MoveToPoint(_dragStart);
			}
		} else if (Input.IsActionJustReleased("LMB")) {
			_dragEnd = _camera.GetGlobalMousePosition();
			_dragEndV = _mousePosition;
			_dragging = false;
			_camera.EnableEdgePanning = true;
			DrawArea(false);
			if (_dragStartV.DistanceTo(_dragEndV) > 10) {
				GD.Print("Area select");
				SelectUnitsInArea(_dragStart, _dragEnd);
			} else {
				SelectUnitNearPoint(_dragStart);
			}
		}

		if (_dragging) {
			_dragEnd = _camera.GetGlobalMousePosition();
			_dragEndV = _mousePosition;
			DrawArea();
		}

		if (BuildingMode && _buildingShade is not null) {
			var pos = _camera.GetGlobalMousePosition();
			var planet = _mapGrid.Planets.Find(planet => planet.PointNearToRing(pos));
			_buildingShade.CalculateBuildingPlace(pos, planet);
		}
	}

	private void CheckSectorMapClick() {
		var a = GetViewport().GetMousePosition();
		var b = _uiController.SectorPanel.GlobalPosition;
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
		var panel = _uiController.SelectionPanel;
		panel.Size = new Vector2(Math.Abs(_dragStartV.X - _dragEndV.X), Math.Abs(_dragStartV.Y - _dragEndV.Y));
		var pos = Vector2.Zero;
		pos.X = Math.Min(_dragStartV.X, _dragEndV.X);
		pos.Y = Math.Min(_dragStartV.Y, _dragEndV.Y);
		panel.Position = pos;
		panel.Size *= s ? 1 : 0;
	}


	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouse mouse) {
			_mousePosition = mouse.Position;
			_mousePosGlobal = _camera.GetGlobalMousePosition();
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

	public void BuildBuilding(BuildingNode2D buildingNode2D) {
		var building = buildingNode2D.Planet.BuildBuilding(buildingNode2D);
		if (building is not null) {
			_buildings.Add(building);
		}
	}

	private void SelectUnitsInArea(Vector2 start, Vector2 end) {
		var shiftDown = Input.IsKeyPressed(Key.Shift);
		BuildingMode = false;

		if (shiftDown) {
			var units = _units.Where(unit => AreaHelper.InRect(unit.Position, start, end)).ToList();
			if (units.Count == 0) {
				_selectedUnits.ForEach(x => x.Selected = false);
				_selectedUnits = new List<UnitNode2D>();
			} else {
				foreach (var unit in units) {
					unit.Selected = !unit.Selected;
				}
				_selectedUnits = _units.Where(x => x.Selected).ToList();
			}
		} else {
			foreach (var unit in _units) {
				unit.Selected = AreaHelper.InRect(unit.Position, start, end);
			}
			_selectedUnits = _units.Where(x => x.Selected).ToList();
		}

		UpdateUi();
	}

	private void SelectUnitNearPoint(Vector2 point) {
		if (_uiController.MouseOverGui(_mousePosition) || BuildingMode) {
			return;
		}

		var shiftDown = Input.IsKeyPressed(Key.Shift);
		if (!shiftDown) {
			_units.ForEach(x => x.Selected = false);
		}

		var unitNode2D = _units.Find(x => x.InsideSelectionRect(point));
		if (unitNode2D is null) {
			_selectedUnits.ForEach(x => x.Selected = false);
			_selectedUnits.Clear();
		} else {
			if (shiftDown) {
				unitNode2D.Selected = !unitNode2D.Selected;
				if (unitNode2D.Selected) {
					_selectedUnits.Add(unitNode2D);
				} else {
					_selectedUnits.Remove(unitNode2D);
				}
			} else {
				unitNode2D.Selected = true;
				_selectedUnits = new List<UnitNode2D> { unitNode2D };
			}
		}

		UpdateUi();
	}

	private void UpdateUi() {
		var units = _selectedUnits;
		LeftControls.SetBuildingContainerVisibility(units.Count == 1 && units[0] is Fabricator);
		LeftControls.CalculateSelectedUnits(units);
	}

	public void SelectUnitFromUi(UnitNode2D unit) {
		var shiftDown = Input.IsKeyPressed(Key.Shift);
		if (shiftDown) {
			unit.Selected = false;
			_selectedUnits.Remove(unit);
		} else {
			_units.ForEach(x => x.Selected = x == unit);
			_selectedUnits = new List<UnitNode2D> { unit };
		}

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
}
