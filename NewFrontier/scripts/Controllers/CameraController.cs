using System;
using Godot;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Controllers;

public partial class CameraController : Camera2D {
	[Signal]
	public delegate void AreaSelectedEventHandler(Vector2 start, Vector2 end);

	[Signal]
	public delegate void MoveToPointEventHandler(Vector2 point);

	[Signal]
	public delegate void PointSelectedEventHandler(Vector2 point);

	[Signal]
	public delegate void StartMoveSelectionEventHandler();

	private const int EdgeSize = 25;

	private MapGrid _mapGrid;
	private UiController _uiController;
	[Export] public bool EnableEdgePanning = true;
	
	public PlayerController PlayerControllerInstance;
	[Export] public float Speed = 10.0f;
	


	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_uiController = GetNode<UiController>("../Ui");
	}

	public void Init(MapGrid mapGrid) {
		_mapGrid = mapGrid;
	}

	public void CenterOnGridPosition(Vector2 pos) {
		CenterOnPosition(MapHelpers.GridCoordToGridCenterPos(pos));
	}

	public void CenterOnPosition(Vector2 pos) {
		Position = pos;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		var inpx = 0;
		var inpy = 0;

		if (EnableEdgePanning) {
			var windowSize = DisplayServer.WindowGetSize();
			var windowPosition = DisplayServer.WindowGetPosition();
			var windowMousePosition = DisplayServer.MouseGetPosition();

			if (windowMousePosition.X - windowPosition.X < EdgeSize) {
				inpx = -1;
			} else if (windowPosition.X + windowSize.X - windowMousePosition.X < EdgeSize) {
				inpx = 1;
			}

			if (windowMousePosition.Y - windowPosition.Y < EdgeSize) {
				inpy = -1;
			} else if (windowPosition.Y + windowSize.Y - windowMousePosition.Y < EdgeSize) {
				inpy = 1;
			}
		}

		// inpx = (Input.IsActionPressed("ui_right") ? 1 : 0) 
		// 		   - (Input.IsActionPressed("ui_left") ? 1 : 0);
		// inpy = (Input.IsActionPressed("ui_down") ? 1 : 0) 
		// 		   - (Input.IsActionPressed("ui_up") ? 1 : 0);
		var offset = MapHelpers.CalculateOffset(0, 0, PlayerControllerInstance.CurrentSector) * MapHelpers.DrawSize;
		var diameter = _mapGrid.RealMapSize * MapHelpers.DrawSize;
		var radius = diameter / 2;
		var center = new Vector2(radius, radius) + offset;
		var size = GetViewport().GetVisibleRect().Size;
		var a = radius - (size.X / 2);
		var b = radius - (size.Y / 2);
		Position += new Vector2(inpx * Speed, inpy * Speed);

		while (IsPointOutsideEllipse(center, a, b, Position)) {
			Position += (center - Position).Normalized();
		}

		Position = Position.Clamp((DisplayServer.WindowGetSize() / 2) + offset,
			new Vector2(diameter, diameter) - (DisplayServer.WindowGetSize() / 2) + offset);
	}

	private bool IsPointOutsideEllipse(Vector2 center, float a, float b, Vector2 point) {
		var distanceSquared = (Math.Pow(point.X - center.X, 2) / Math.Pow(a, 2)) +
							  (Math.Pow(point.Y - center.Y, 2) / Math.Pow(b, 2));
		return distanceSquared > 1;
	}

}
