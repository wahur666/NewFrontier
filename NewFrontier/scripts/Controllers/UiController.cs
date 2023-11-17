using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Controllers;

public partial class UiController : CanvasLayer {
	private PlayerController _playerController;
	public Node2D Canvas;
	private List<Control> canvasItems;
	private LeftControls leftControls;
	public Panel SectorPanel;
	public Panel SelectionPanel;
	private MapGrid _mapGrid;

	#region SectorPanel constants

	private Vector2 _sectorPanelGlobalPosition;
	private Vector2 _sectorPanelSize;
	private Vector2 _sectorPanelGlobalPositionBottomLeft;

	#endregion

	private Vector2 _leftPanelGlobalPosition;
	private Vector2 _leftPanelSize;
	private Vector2 _leftPanelGlobalPositionBottomLeft;

	public override void _Ready() {
		canvasItems = new List<Control>();
		leftControls = GetNode<LeftControls>("LeftControls");
		Canvas = GetNode<Node2D>("../Canvas");
		SectorPanel = GetNode<Panel>("SectorMap/Control/Panel");
		SectorPanel.Draw += SectorPanelOnDraw;
		SelectionPanel = GetNode<Panel>("SelectionRect");
		SetupSectorPanelConstants();
		SetupLeftPanelConstants();
	}

	private byte CurrentSector => _playerController.CurrentSector;

	private void SectorPanelOnDraw() {
		var shorterSide = Math.Min(SectorPanel.Size.X, SectorPanel.Size.Y);
		var center = new Vector2(shorterSide, shorterSide) / 2;
		var radius = shorterSide / 2;
		SectorPanel.DrawArc(center, radius, 0, Mathf.Tau, 32, Colors.Azure, 2, true);
		DrawSectors(SectorPanel);
	}

	public void Init(PlayerController playerController, MapGrid mapGrid) {
		_playerController = playerController;
		leftControls.Init(playerController);
		_mapGrid = mapGrid;
	}

	public override void _Process(double delta) {
		Canvas.QueueRedraw();
		SectorPanel.QueueRedraw();
	}

	private static Vector2 CalculatePointOnCircle(Vector2 pos1, Vector2 pos2, float radius) {
		var angle = pos1.AngleToPoint(pos2);
		float x = radius * Mathf.Cos(angle);
		float y = radius * Mathf.Sin(angle);
		return new Vector2(x, y) + pos1;
	}

	private void SetupLeftPanelConstants() {
		_leftPanelGlobalPosition = leftControls.Bg.GlobalPosition;
		_leftPanelSize = leftControls.Bg.Size;
		_leftPanelGlobalPositionBottomLeft = _leftPanelGlobalPosition + _leftPanelSize;
	}

	public bool MouseOverLeftPanel(Vector2 mousePosition) =>
		AreaHelper.InRect(mousePosition, _leftPanelGlobalPosition, _leftPanelGlobalPositionBottomLeft);

	private void SetupSectorPanelConstants() {
		_sectorPanelGlobalPosition = SectorPanel.GlobalPosition;
		_sectorPanelSize = SectorPanel.Size;
		_sectorPanelGlobalPositionBottomLeft = _sectorPanelGlobalPosition + _sectorPanelSize;
	}

	public bool MouseOverSectorMap(Vector2 mousePosition) =>
		AreaHelper.InRect(mousePosition, _sectorPanelGlobalPosition, _sectorPanelGlobalPositionBottomLeft);

	public bool MouseOverGui(Vector2 mousePosition) =>
		MouseOverSectorMap(mousePosition) || MouseOverLeftPanel(mousePosition);

	private void DrawSectors(CanvasItem sectorPanel) {
		const int innerPointRadius = 4;
		const int pointRadius = 7;
		const int enemyPointRadius = 10;
		const int selectorPointRadius = 12;

		foreach (var sector in _mapGrid.Sectors.Where(sector => sector.Discovered)) {
			switch (sector.SectorBuildingStatus) {
				case SectorBuildingStatus.NoBuilding:
					sectorPanel.DrawArc(sector.SectorPosition, pointRadius, 0, Mathf.Tau, 32, Colors.Gray, 2);
					break;
				case SectorBuildingStatus.HasBuilding:
					var color = sector.SupplyLineForSector ? Colors.DodgerBlue : Colors.Yellow;
					sectorPanel.DrawArc(sector.SectorPosition, pointRadius, 0, Mathf.Tau, 32, color, 2);
					sectorPanel.DrawCircle(sector.SectorPosition, innerPointRadius, color);
					break;
				case SectorBuildingStatus.HqBuilding:
					sectorPanel.DrawCircle(sector.SectorPosition, pointRadius, Colors.DodgerBlue);
					break;
				default: break;
			}

			if (sector.EnemyInSector) {
				sectorPanel.DrawArc(sector.SectorPosition, enemyPointRadius, 0, Mathf.Tau, 32, Colors.Red, 3);
			}
		}

		foreach (var mapGridWormhole in _mapGrid.Wormholes) {
			var sector1 = _mapGrid.Sectors.Find(x => x.Index == mapGridWormhole.GetNode1Sector);
			var sector2 = _mapGrid.Sectors.Find(x => x.Index == mapGridWormhole.GetNode2Sector);
			if (sector1 is null || sector2 is null) {
				continue;
			}

			var midPoint = (sector1.SectorPosition + sector2.SectorPosition) / 2;
			var color = mapGridWormhole.SectorJumpGateStatus switch {
				SectorJumpGateStatus.AllyJumpGate => Colors.DodgerBlue,
				SectorJumpGateStatus.EnemyJumpGate => Colors.Red,
				SectorJumpGateStatus.NoJumpGate => Colors.Gray,
				SectorJumpGateStatus.Highlighted => Colors.White,
				_ => Colors.Gray
			};

			if (sector1.Discovered) {
				var pos = CalculatePointOnCircle(sector1.SectorPosition, midPoint, pointRadius);
				sectorPanel.DrawLine(pos, midPoint, color, 2);
			}

			if (sector2.Discovered) {
				var pos = CalculatePointOnCircle(sector2.SectorPosition, midPoint, pointRadius);
				sectorPanel.DrawLine(pos, midPoint, color, 2);
			}
		}

		var currentSector = _mapGrid.Sectors.Find(x => x.Index == CurrentSector);
		if (currentSector is null) {
			return;
		}

		sectorPanel.DrawArc(currentSector.SectorPosition, selectorPointRadius, 0, Mathf.Tau, 32, Colors.White, 2);
	}
}
