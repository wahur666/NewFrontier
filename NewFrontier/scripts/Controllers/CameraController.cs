using Godot;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Controllers;

public partial class CameraController : Camera2D {
	private const int EdgeSize = 25;
	private MapGrid _mapGrid;
	private UiController _uiController;

	[Export] public bool EnableEdgePanning = true;
	public PlayerController PlayerControllerInstance;
	[Export] public float Speed = 10.0f;

	[Export] public float ZoomSpeed = 0.1f;


	public override void _Ready() {
		_uiController = GetNode<UiController>("../Ui");
	}

	public override void _Notification(int what) {
		if (what == MainLoop.NotificationApplicationFocusOut) {
			EnableEdgePanning = false;
		} else if (what == MainLoop.NotificationApplicationFocusIn) {
			EnableEdgePanning = true;
		}
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

	public override void _Process(double delta) {
		var inpx = 0;
		var inpy = 0;


		inpx = (Input.IsActionPressed("ui_right") ? 1 : 0) 
		 		   - (Input.IsActionPressed("ui_left") ? 1 : 0);
		inpy = (Input.IsActionPressed("ui_down") ? 1 : 0) 
		 		   - (Input.IsActionPressed("ui_up") ? 1 : 0);


		if (inpx == 0 && inpy == 0 && EnableEdgePanning) {
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

		var oldZoom = Zoom;
		if (Input.IsActionJustPressed("WheelUp")) {
			Zoom += new Vector2(ZoomSpeed, ZoomSpeed);
			Zoom = Zoom.Clamp(new Vector2(0.5f, 0.5f), new Vector2(1.5f, 1.5f));
		} else if (Input.IsActionJustPressed("WheelDown")) {
			Zoom -= new Vector2(ZoomSpeed, ZoomSpeed);
			Zoom = Zoom.Clamp(new Vector2(0.5f, 0.5f), new Vector2(1.5f, 1.5f));
		}

		if (inpx == 0 && inpy == 0 && oldZoom != Zoom) {
			return;
		}

		var offset = MapHelpers.CalculateOffset(0, 0, PlayerControllerInstance.CurrentSector) * MapHelpers.DrawSize;
		var diameter = _mapGrid.RealMapSize * MapHelpers.DrawSize;
		var radius = diameter / 2;
		var center = new Vector2(radius, radius) + offset;
		var size = GetViewport().GetVisibleRect().Size / Zoom * 0.9f;
		var a = radius - (size.X / 2);
		var b = radius - (size.Y / 2);
		Position += new Vector2(inpx * Speed, inpy * Speed);

		while (AreaHelper.IsPointOutsideEllipse(center, a, b, Position)) {
			Position += (center - Position).Normalized();
		}

		Position = Position.Clamp((DisplayServer.WindowGetSize() / 2) + offset,
			new Vector2(diameter, diameter) - (DisplayServer.WindowGetSize() / 2) + offset);
	}
}
