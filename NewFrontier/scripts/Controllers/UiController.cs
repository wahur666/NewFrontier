using System;
using System.Linq;
using Godot;
using NewFrontier.scripts.Entities;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Controllers;

public partial class UiController : CanvasLayer {
	private Vector2 _leftPanelGlobalPosition;
	private Vector2 _leftPanelGlobalPositionBottomLeft;
	private Vector2 _leftPanelSize;
	private MapGrid _mapGrid;
	private PlayerController _playerController;
	public Node2D Canvas;
	private LeftControls leftControls;
	public Panel SelectionPanel;
	public PlayerStatsUi PlayerStatsUi;
	public PlanetUi PlanetUi;
	public SectorMap SectorMap;
	public SectorMap MiniMap;

	public override void _Ready() {
		leftControls = GetNode<LeftControls>("LeftControls");
		PlayerStatsUi = GetNode<PlayerStatsUi>("Stats");
		Canvas = GetNode<Node2D>("../Canvas");
		SectorMap = GetNode<SectorMap>("SectorMap");
		MiniMap = GetNode<SectorMap>("MiniMap");
		SelectionPanel = GetNode<Panel>("SelectionRect");
		PlanetUi = GetNode<PlanetUi>("PlanetUi");
		SetupLeftPanelConstants();
	}


	public void Init(PlayerController playerController, CameraController cameraController, MapGrid mapGrid) {
		_playerController = playerController;
		leftControls.Init(playerController);
		SectorMap.Init(playerController, cameraController, mapGrid);
		MiniMap.Init(playerController, cameraController, mapGrid);
		_mapGrid = mapGrid;
	}

	public override void _Process(double delta) {
		Canvas.QueueRedraw();
	}

	// TODO: Move this to left panel
	private void SetupLeftPanelConstants() {
		_leftPanelGlobalPosition = leftControls.Bg.GlobalPosition;
		_leftPanelSize = leftControls.Bg.Size;
		_leftPanelGlobalPositionBottomLeft = _leftPanelGlobalPosition + _leftPanelSize;
	}

	public bool MouseOverLeftPanel(Vector2 mousePosition) {
		return AreaHelper.InRect(mousePosition, _leftPanelGlobalPosition, _leftPanelGlobalPositionBottomLeft);
	}

	public bool MouseOverGui(Vector2 mousePosition) {
		return SectorMap.MouseOverSectorMap(mousePosition) || MouseOverLeftPanel(mousePosition);
	}
}
