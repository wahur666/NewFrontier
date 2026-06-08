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
	[Export] public float Speed = 20.0f;

	[Export] public float ZoomSpeed = 0.1f;


	public override void _Ready() {
		_uiController = GetNode<UiController>("../Ui");
	}

	public Rect2 GetVisibleWorldRect() {
		var size = GetViewport().GetVisibleRect().Size / Zoom;
		var topLeft = GetScreenCenterPosition() - size / 2f;
		return new Rect2(topLeft, size);
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
		CenterOnPosition(MapHelpers.GridCellToWorldCenter(pos));
	}

	public void CenterOnPosition(Vector2 pos) {
		Position = pos;
	}

	public void SetBoundedPosition(Vector2 position) {
		Position = ClampPositionToCurrentSector(position);
	}

	public Vector2 ClampPositionToCurrentSector(Vector2 position) {
		if (_mapGrid is null || PlayerControllerInstance is null) {
			return position;
		}

		var offset = MapHelpers.GridLineToWorldPoint(
			MapHelpers.SectorLocalGridToGlobalGrid(0, 0, PlayerControllerInstance.CurrentSector)
		);
		var radius = (PlayerControllerInstance.CurrentSectorObj.Size - 0.5f) * MapHelpers.DrawSize;
		var center = new Vector2(radius, radius) + offset;
		var halfViewSize = GetViewport().GetVisibleRect().Size / Zoom * 0.45f;
		var a = radius - halfViewSize.X;
		var b = radius - halfViewSize.Y;

		if (a <= 0 || b <= 0) {
			return center;
		}

		if (a > 0 && b > 0 && AreaHelper.IsPointOutsideEllipse(center, a, b, position)) {
			var fromCenter = position - center;
			var ellipseScale = Mathf.Sqrt(Mathf.Pow(fromCenter.X / a, 2) + Mathf.Pow(fromCenter.Y / b, 2));
			position = center + fromCenter / ellipseScale;
		}

		return position.Clamp(center - new Vector2(a, b), center + new Vector2(a, b));
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
			Zoom = Zoom.Clamp(new Vector2(0.5f, 0.5f), new Vector2(1.0f, 1.0f));
		} else if (Input.IsActionJustPressed("WheelDown")) {
			Zoom -= new Vector2(ZoomSpeed, ZoomSpeed);
			Zoom = Zoom.Clamp(new Vector2(0.5f, 0.5f), new Vector2(1.0f, 1.0f));
		}

		if (inpx == 0 && inpy == 0 && oldZoom != Zoom) {
			SetBoundedPosition(Position);
			return;
		}

		Position += new Vector2(inpx * Speed, inpy * Speed);
		SetBoundedPosition(Position);
	}
}
