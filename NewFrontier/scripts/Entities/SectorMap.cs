using System;
using System.Linq;
using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Entities;

public partial class SectorMap : Control {
	private Vector2 _sectorPanelGlobalPosition;
	private Vector2 _sectorPanelSize;
	public Panel SectorPanel { get; private set; }

	private Vector2 _sectorPanelGlobalPositionBottomLeft;
	private MapGrid _mapGrid;
	private PlayerController _playerController;

	public override void _Ready() {
		SectorPanel = GetNode<Panel>("Control/SectorPanel");
		SectorPanel.Draw += SectorPanelOnDraw;
		SetupSectorPanelConstants();
	}

	public override void _Process(double delta) {
		SectorPanel.QueueRedraw();
	}

	public void Init(PlayerController playerController, MapGrid mapGrid) {
		_playerController = playerController;
		_mapGrid = mapGrid;
	}
	
	private void SetupSectorPanelConstants() {
		_sectorPanelGlobalPosition = SectorPanel.GlobalPosition;
		_sectorPanelSize = SectorPanel.Size;
		_sectorPanelGlobalPositionBottomLeft = _sectorPanelGlobalPosition + _sectorPanelSize;
	}

	public bool MouseOverSectorMap(Vector2 mousePosition) {
		return AreaHelper.InRect(mousePosition, _sectorPanelGlobalPosition, _sectorPanelGlobalPositionBottomLeft);
	}
	
	private void SectorPanelOnDraw() {
		var shorterSide = Math.Min(SectorPanel.Size.X, SectorPanel.Size.Y);
		var center = new Vector2(shorterSide, shorterSide) / 2;
		var radius = shorterSide / 2;
		SectorPanel.DrawArc(center, radius, 0, Mathf.Tau, 32, Colors.Azure, 2, true);
		DrawSectors(SectorPanel);
	}
	
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
			}

			if (sector.EnemyInSector) {
				sectorPanel.DrawArc(sector.SectorPosition, enemyPointRadius, 0, Mathf.Tau, 32, Colors.Red, 3);
			}
		}

		foreach (var mapGridWormhole in _mapGrid.WormholeObjects) {
			var sector1 = _mapGrid.GetSector(mapGridWormhole.GetNode1Sector);
			var sector2 = _mapGrid.GetSector(mapGridWormhole.GetNode2Sector);
			if (sector1 is null || sector2 is null) {
				continue;
			}

			var midPoint = (sector1.SectorPosition + sector2.SectorPosition) / 2;
			var color = mapGridWormhole.Highlighted ? Colors.White : mapGridWormhole.SectorJumpGateStatus switch {
				SectorJumpGateStatus.AllyJumpGate => Colors.DodgerBlue,
				SectorJumpGateStatus.EnemyJumpGate => Colors.Red,
				SectorJumpGateStatus.NoJumpGate => Colors.Gray,
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

		sectorPanel.DrawArc(_playerController.CurrentSectorObj.SectorPosition, selectorPointRadius, 0, Mathf.Tau, 32,
			Colors.White, 2);
	}
	
	private static Vector2 CalculatePointOnCircle(Vector2 pos1, Vector2 pos2, float radius) {
		var angle = pos1.AngleToPoint(pos2);
		var x = radius * Mathf.Cos(angle);
		var y = radius * Mathf.Sin(angle);
		return new Vector2(x, y) + pos1;
	}

}
