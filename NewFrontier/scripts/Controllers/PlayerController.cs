using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Entities;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Factions;
using NewFrontier.scripts.Model.Interfaces;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Controllers;

public partial class PlayerController : Node {
	private readonly List<BuildingNode2D> _buildings = [];
	private Node _buildingContainer;

	private bool _buildingMode;
	private IBuildable _buildingShade;
	private CameraController _camera;

	private byte _currentSector;

	private MapGrid _mapGrid;
	private bool _overGui;
	private PlayerStats _playerStats = new();
	private List<ISelectable> _selectedObjects = [];

	private UiController _uiController;
	private List<UnitNode2D> _units = [];
	private bool _wormholeClick;

	public Vector2 GlobalMousePosition { get => _camera.GetGlobalMousePosition(); }


	public LeftControls LeftControls;

	public byte CurrentSector {
		get => _currentSector;
		private set {
			_currentSector = value;
			CurrentSectorObj = _mapGrid.GetSector(_currentSector);
		}
	}

	public Sector CurrentSectorObj { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_buildingContainer = GetNode<Node>("BuildingsContainer");
		_camera = GetNode<CameraController>("../../Camera2D");
		_camera.PlayerControllerInstance = this;
		_mapGrid = GetNode<MapGrid>("../../MapGrid");
		_uiController = GetNode<UiController>("../../Ui");
		_uiController.Init(this, _mapGrid);
		CreateStartingUnits();
		_camera.CenterOnGridPosition(new Vector2(12, 17));
		CurrentSector = 0;
		_uiController.PlayerStatsUi.UpdateLabels(_playerStats);
		UpdateUi();
	}

	private void CreateStartingUnits() {
		_units = [];
		var harvester = FactionController.Terran.CreateHarvester(new Vector2(10, 10), this, _uiController, _mapGrid);
		_units.Add(harvester);
		var fabricator = FactionController.Terran.CreateFabricator(new Vector2(12, 17), this, _uiController, _mapGrid);
		_units.Add(fabricator);
		var dreadnought =
			FactionController.Terran.CreateDreadnought(new Vector2(12, 19), this, _uiController, _mapGrid);
		_units.Add(dreadnought);
		_units.ForEach(e => AddChild(e));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("LMB")) {
			_dragStart = _camera.GetGlobalMousePosition();
			_dragStartV = _mousePosition;
			if (_buildingMode && _buildingShade is not null) {
				if (_buildingShade.Planet is not null && _buildingShade.SnapOption == SnapOption.Planet) {
					BuildBuildingOnPlanet(_buildingShade);
				} else if (_buildingShade.SnapOption == SnapOption.Grid) {
					BuildBuildingOnGrid(_buildingShade);
				} else if (_buildingShade.SnapOption == SnapOption.Wormhole) {
					BuildBuildingOnWormholes(_buildingShade);
				}
			} else if (_uiController.SectorMap.MouseOverSectorMap(_mousePosition)) {
				CheckSectorMapClick(_mousePosition);
			} else if (_mapGrid.Wormholes.Any(x => x.MousePointerIsOver)) {
				_wormholeClick = true;
				SwitchCameraToJoinedWormhole();
			} else {
				_dragging = true;
				_camera.EnableEdgePanning = false;
			}
		} else if (Input.IsActionJustPressed("CTRL+RMB")) {
			if (SetTarget()) {
				return;
			}
		} else if (Input.IsActionJustPressed("RMB")) {
			if (_buildingMode) {
				FreeBuildingShade();
				_buildingMode = false;
			} else if (_uiController.SectorMap.MouseOverSectorMap(_mousePosition)) {
				NavigateToSectorFromSectorMap(_mousePosition);
			} else {
				_dragStart = _camera.GetGlobalMousePosition();
				_dragStartV = _mousePosition;
				_dragging = false;
				_camera.EnableEdgePanning = true;
				DrawArea(false);
				var units = _units.Where(x => x.Selected).ToList();
				MoveToPoint(units, _camera.GetGlobalMousePosition());
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

			_wormholeClick = false;
		}


		if (_dragging) {
			_dragEnd = _camera.GetGlobalMousePosition();
			_dragEndV = _mousePosition;
			DrawArea();
		}

		// Updates the building shade position every frame
		if (_buildingMode && _buildingShade is not null) {
			var pos = _camera.GetGlobalMousePosition();
			BuildHelper.CalculateBuildingPlace(_buildingShade, _mapGrid, pos);
		}

		UpdatePlanetUi();

		CurrentSectorObj.CameraPosition = _camera.Position;
	}

	private void NavigateToSectorFromSectorMap(Vector2 mousePosition) {
		var units = _units.Where(x => x.Selected).ToList();
		var sectorPanelGlobalPosition = _uiController.SectorMap.SectorPanel.GlobalPosition;
		var sector = _mapGrid.Sectors.Where(x => x.Discovered).ToList()
			.Find(x => Math.Abs((x.SectorPosition + sectorPanelGlobalPosition - mousePosition).Length()) < 10);
		var endPosition = _mapGrid.FindFreePosition(_mapGrid.GetGameNode(sector.CenterPosition()), false).First().PositionI;
		var targetGlobalPosition = MapHelpers.GridCoordToGridPointPos(endPosition);
		MoveToPoint(units, targetGlobalPosition);
	}

	private bool SetTarget() {
		if (_uiController.MouseOverGui(_mousePosition) || _buildingMode || _wormholeClick) {
			return true;
		}

		var point = _camera.GetGlobalMousePosition();
		if (AllSelectable.Find(x => x.InsideSelectionRect(point)) is not IBase baseObject) {
			return false;
		}

		foreach (var selectedObject in _selectedObjects) {
			GD.Print("TARGET AQUIRED");
			((IAttack)selectedObject).Target = baseObject;
		}

		return false;
	}

	private void UpdatePlanetUi() {
		var planet = _mapGrid.Planets.Find(planet => planet.MouseOver);
		_uiController.PlanetUi.Visible = planet is not null;
		if (planet is not null) {
			_uiController.PlanetUi.UpdateLabels(planet);
		}
	}

	private void SwitchCameraToJoinedWormhole() {
		var wormhole = _mapGrid.Wormholes.Find(x => x.MousePointerIsOver);
		var who = _mapGrid.WormholeObjects.Find(x => x.IsConnected(wormhole.GameNode));
		var otherNode = who.GetOtherNode(wormhole.GameNode);
		var sector = _mapGrid.GetSector(otherNode.SectorIndex);
		if (sector?.Discovered == true) {
			CurrentSector = otherNode.SectorIndex;
			_camera.CenterOnGridPosition(otherNode.Position);
		}
	}

	private void CheckSectorMapClick(Vector2 mousePosition) {
		var sectorPanelGlobalPosition = _uiController.SectorMap.SectorPanel.GlobalPosition;
		var sector = _mapGrid.Sectors.Where(x => x.Discovered).ToList()
			.Find(x => Math.Abs((x.SectorPosition + sectorPanelGlobalPosition - mousePosition).Length()) < 10);
		if (sector is null) {
			return;
		}

		CurrentSector = sector.Index;
		_camera.Position = sector.CameraPosition;
	}

	private void DrawArea(bool s = true) {
		var panel = _uiController.SelectionPanel;
		panel.Size = new Vector2(Math.Abs(_dragStartV.X - _dragEndV.X), Math.Abs(_dragStartV.Y - _dragEndV.Y));
		var pos = Vector2.Zero;
		pos.X = Math.Min(_dragStartV.X, _dragEndV.X);
		pos.Y = Math.Min(_dragStartV.Y, _dragEndV.Y);
		panel.Position = pos;
		panel.Size *= s ? 1 : 0;
	}

	private record PathCache(
		UnitNode2D unit,
		Vector2 position,
		Sector sector,
		List<GameNode> path
	);

	private void ClearPathCache() {
		_pathCache = new(null, Vector2.Zero, null, []);
	}

	private PathCache _pathCache = new(null, Vector2.Zero, null, []);

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouse mouse) {
			_mousePosition = mouse.Position;
			_mousePosGlobal = _camera.GetGlobalMousePosition();
			CheckSectorMapHoverWithUnit();
		}
	}

	private void CheckSectorMapHoverWithUnit() {
		if (CheckSectorMap(_mousePosition) is { } sector) {
			if (_selectedObjects.Count <= 0
			    || _selectedObjects[^1] is not UnitNode2D unitNode2D) {
				return;
			}

			if (_pathCache.unit == unitNode2D
			    && _pathCache.position == unitNode2D.GridPosition()
			    && _pathCache.sector == sector) {
				return;
			}

			var start = _mapGrid.GetGameNode(unitNode2D.GridPosition());
			var end = _mapGrid.GetGameNode(sector.CenterPosition());

			if (start.SectorIndex == end.SectorIndex) {
				return;
			}

			var path = _mapGrid.Navigation.FindPath(start, end, disableOptimisation: true).ToList();
			_pathCache = new(unitNode2D, unitNode2D.GridPosition(), sector, path);

			var wormholes = _pathCache.path.Where(node => node.HasWormhole).ToList();
			_mapGrid.WormholeObjects.ForEach(wo => wo.Highlighted = false);
			foreach (var wormholeObject in wormholes
				         .SelectMany(wormhole => _mapGrid.WormholeObjects
					         .Where(wormholeObject => wormholeObject.IsConnected(wormhole)))) {
				wormholeObject.Highlighted = true;
			}
		} else {
			if (_pathCache.unit is null) {
				return;
			}

			_mapGrid.WormholeObjects.ForEach(wo => wo.Highlighted = false);
			ClearPathCache();
		}
	}

	private Sector CheckSectorMap(Vector2 position) {
		var sectorPanelPosition = _uiController.SectorMap.SectorPanel.GlobalPosition;
		return _mapGrid.Sectors
			.Where(x => x.Discovered)
			.ToList()
			.Find(sec => Math.Abs((sec.SectorPosition + sectorPanelPosition - position).Length()) < 10);
	}


	private void FreeBuildingShade() {
		_buildingShade?.Instance.QueueFree();
		_buildingShade = null;
	}

	public void SetBuildingShadeVisibility(bool visible) {
		if (_buildingShade is not null) {
			_buildingShade.Instance.Visible = visible;
		}
	}

	public void CreateBuilding(string name, Func<PlayerController, IBuildable> func) {
		_buildingMode = true;
		if (_buildingShade?.BuildingName == name) {
			return;
		}

		FreeBuildingShade();

		_buildingShade = func(this);
		_buildingShade.Instance.Visible = false;
		AddChild(_buildingShade.Instance);
	}

	private List<ISelectable> AllSelectable => [.._units, .._buildings];


	private void SelectUnitsInArea(Vector2 start, Vector2 end) {
		if (_uiController.MouseOverGui(_mousePosition) || _buildingMode || _wormholeClick) {
			return;
		}

		var shiftDown = Input.IsKeyPressed(Key.Shift);

		var units = AllSelectable.Where(unit => AreaHelper.InRect(unit.Pos, start, end)).ToList();
		if (shiftDown && units.Count == 0) {
			ClearSelectedObjects();
			UpdateUi();
			return;
		}

		if (shiftDown) {
			foreach (var unit in units) {
				unit.Selected = !unit.Selected;
			}
		} else {
			foreach (var unit in AllSelectable) {
				unit.Selected = AreaHelper.InRect(unit.Pos, start, end);
			}
		}

		_selectedObjects = AllSelectable.Where(x => x.Selected).ToList();

		if (_selectedObjects.Any(unit => unit.IsUnitSelectable) &&
		    _selectedObjects.Any(unit => !unit.IsUnitSelectable)) {
			foreach (var selectedObject in _selectedObjects.Where(selectedObject => !selectedObject.IsUnitSelectable)) {
				selectedObject.Selected = false;
			}

			_selectedObjects = _selectedObjects.Where(unit => unit.Selected).ToList();
		}

		UpdateUi();
	}

	private void ClearSelectedObjects() {
		_selectedObjects.ForEach(x => x.Selected = false);
		_selectedObjects.Clear();
	}

	private void SelectUnitNearPoint(Vector2 point) {
		if (_uiController.MouseOverGui(_mousePosition) || _buildingMode || _wormholeClick) {
			return;
		}

		var shiftDown = Input.IsKeyPressed(Key.Shift);

		var selectedObject = AllSelectable.Find(x => x.InsideSelectionRect(point));
		if (selectedObject is null) {
			ClearSelectedObjects();
			UpdateUi();
			return;
		}

		if (shiftDown) {
			if (selectedObject.IsUnitSelectable) {
				// Unit Selected
				if (_selectedObjects.All(x => x.IsUnitSelectable)) {
					// Only  units we have here
					selectedObject.Selected = !selectedObject.Selected;
					if (selectedObject.Selected) {
						_selectedObjects.Add(selectedObject);
					} else {
						_selectedObjects.Remove(selectedObject);
					}
				} else {
					// We have a building in the list
					ClearSelectedObjects();
				}
			}
		} else {
			_selectedObjects.ForEach(x => x.Selected = false);
			selectedObject.Selected = true;
			_selectedObjects = [selectedObject];
		}

		UpdateUi();
	}

	private void UpdateUi() {
		var selectedObjects = _selectedObjects;
		var isFabricatorSelected = selectedObjects.Count == 1 && selectedObjects[0] is Fabricator;
		if (isFabricatorSelected) {
			LeftControls.SetContainerVisibility(false, true, false);
			return;
		}

		var isBuildingSelected = selectedObjects.Count == 1 && selectedObjects[0] is BuildingNode2D;
		if (isBuildingSelected) {
			LeftControls.SetContainerVisibility(false, false, true);
			return;
		}

		LeftControls.SetContainerVisibility(true, false, false);
		LeftControls.CalculateSelectedUnits(selectedObjects);
	}

	public void SelectUnitFromUi(ISelectable unit) {
		var shiftDown = Input.IsKeyPressed(Key.Shift);
		if (shiftDown) {
			unit.Selected = false;
			_selectedObjects.Remove(unit);
		} else {
			_units.ForEach(x => x.Selected = x == unit);
			_selectedObjects = [unit];
		}

		UpdateUi();
	}

	private void MoveToPoint(UnitNode2D unit, Vector2 mouseGlobalPosition) => MoveToPoint([unit], mouseGlobalPosition);

	private void MoveToPoint(List<UnitNode2D> units, Vector2 mouseGlobalPosition) {
		if (units.Count == 0) {
			return;
		}

		var mouseEndVector = MapHelpers.PosToGrid(mouseGlobalPosition);
		var mouseEndNode = _mapGrid.GridLayer[mouseEndVector.X, mouseEndVector.Y];
		if (mouseEndNode is null) {
			return;
		}

		var unitSectors = units
			.Select(x => MapHelpers.GetSectorIndexFromOffset(x.GridPosition(x.GlobalPosition)))
			.ToHashSet();
		if (mouseEndNode.HasWormhole) {
			if (unitSectors.Count > 1) {
				return;
			}

			if (mouseEndNode.SectorIndex != unitSectors.First()) {
				return;
			}
		}

		foreach (var unitNode2D in units) {
			if (mouseEndNode.PreOccupied is not null && mouseEndNode.PreOccupied == unitNode2D) {
				continue;
			}

			_mapGrid.ClearUnitPreOccupation(unitNode2D);

			var startVector2 = unitNode2D.GridPosition(unitNode2D.GlobalPosition);
			var start = _mapGrid.GridLayer[(int)startVector2.X, (int)startVector2.Y];
			var endVector = unitNode2D.BigShip
				? MapHelpers.PosToGridPoint(mouseGlobalPosition)
				: MapHelpers.PosToGrid(mouseGlobalPosition);
			var end = _mapGrid.GridLayer[endVector.X, endVector.Y];

			if (end.HasWormhole) {
				var random = new Random();
				var otherNode = _mapGrid.GetConnectedWormholeNode(end.WormholeNode);
				var arr = otherNode.Neighbours.Keys.Where(x => !x.HasWormhole).ToArray();
				end = arr[random.Next(arr.Length)];
			}

			List<GameNode> nodes = _mapGrid.FindFreePosition(end, unitNode2D.BigShip);
			if (nodes.Count > 0) {
				nodes.ForEach(x => x.PreOccupied = unitNode2D);
				end = nodes[^1];
			} else {
				continue;
			}

			if (end == start) {
				continue;
			}


			var path = new List<GameNode>();
			if (start is not null) {
				path = _mapGrid.Navigation
					.FindPath(start, end)
					.ToList();
			}

			unitNode2D.SetNavigation(path);
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

	public void IncreaseOre(int amount) {
		_playerStats.CurrentOre += amount;
		_uiController.PlayerStatsUi.UpdateLabels(_playerStats);
	}

	public void IncreaseGas(int amount) {
		_playerStats.CurrentGas += amount;
		_uiController.PlayerStatsUi.UpdateLabels(_playerStats);
	}

	public void IncreaseCrew(int amount) {
		_playerStats.CurrentCrew += amount;
		_uiController.PlayerStatsUi.UpdateLabels(_playerStats);
	}

	#region Building Code

	private void BuildBuildingOnPlanet(IBuildable buildingNode2D) {
		BuildingNode2D building = null;
		if (buildingNode2D.SnapOption == SnapOption.Planet) {
			building = buildingNode2D.Planet.BuildBuilding(buildingNode2D);
		}

		if (building is not null) {
			building.Init(this, buildingNode2D.BuildingName);
			building.Planet = buildingNode2D.Planet;
			_buildings.Add(building);
		}
	}

	private void BuildBuildingOnWormholes(IBuildable buildingNode2D) {
		var mousePos = _camera.GetGlobalMousePosition();
		var freeSpace = _mapGrid.FreeSpace(mousePos, 2, true);
		if (freeSpace is null) {
			return;
		}

		var gridPos = freeSpace.Value;
		var wormholes = _mapGrid.GetWormholes(gridPos);
		var wormholeOccupiedNodes = _mapGrid.GetWormholeOccupiedNodes(wormholes).ToList();
		wormholeOccupiedNodes.ForEach(node => node.Occupied = true);

		var buildings = wormholes.Select(wormhole => wormhole.Build(buildingNode2D)).ToList();

		_buildings.AddRange(buildings);
	}

	private void BuildBuildingOnGrid(IBuildable buildingNode2D) {
		var mousePos = _camera.GetGlobalMousePosition();
		var freeSpace = buildingNode2D.Wide switch {
			1 => _mapGrid.FreeSpace(mousePos, 1),
			2 => _mapGrid.FreeSpace(mousePos, 2),
			_ => null
		};

		if (freeSpace is null) {
			return;
		}

		if (buildingNode2D.Instance.Duplicate() is not BuildingNode2D building) {
			return;
		}

		building.BuildingShade = false;

		var gridPos = freeSpace.Value;
		if (building.Wide == 1) {
			building.GlobalPosition = MapHelpers.GridCoordToGridCenterPos(gridPos);
			_mapGrid[(int)gridPos.X, (int)gridPos.Y].Occupied = true;
		} else if (buildingNode2D.Wide == 2) {
			building.GlobalPosition = MapHelpers.GridCoordToGridPointPos(freeSpace.Value);
			_mapGrid[(int)gridPos.X - 1, (int)gridPos.Y - 1].Occupied = true;
			_mapGrid[(int)gridPos.X - 1, (int)gridPos.Y].Occupied = true;
			_mapGrid[(int)gridPos.X, (int)gridPos.Y - 1].Occupied = true;
			_mapGrid[(int)gridPos.X, (int)gridPos.Y].Occupied = true;
		}

		_buildings.Add(building);
		_buildingContainer.AddChild(building);
	}

	#endregion

	#region Dragging variables

	private Vector2 _dragEnd;
	private Vector2 _dragEndV;
	private Vector2 _mousePosGlobal;
	private Vector2 _dragStart;
	private Vector2 _dragStartV;
	private Vector2 _mousePosition;
	private bool _dragging;

	#endregion

	public void CreateUnit(string unit) {
		if (unit == Terran.Harvester) {
			if (_selectedObjects[0] is Refinery refinery) {
				var harvester =
					FactionController.Terran.CreateHarvester(
						MapHelpers.PosToGrid(refinery.BuildLocation.GlobalPosition),
						this, _uiController,
						_mapGrid);
				this._units.Add(harvester);
				AddChild(harvester);
				MoveToPoint(harvester, refinery.BuildLocation.GlobalPosition);
			}
		}
	}

	public void RemoveUnit(UnitNode2D unitNode2D) {
		_units.Remove(unitNode2D);
	}

	public void MarkSectorDiscovered(byte sectorIndex) {
		_mapGrid.Sectors[sectorIndex].Discovered = true;
	}
}
