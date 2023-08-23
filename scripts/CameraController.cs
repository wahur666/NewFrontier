using Godot;
using System;


namespace NewFrontier.scripts;

public partial class CameraController : Camera2D {
	[Export] public float Speed = 10.0f;
	[Export] public bool EnableEdgePanning = true;
	private Vector2 _windowSize = Vector2.Zero;

	private bool _dragging = false;

	private Vector2 mousePosition;
	private Vector2 mousePosGlobal;
	private Vector2 start;
	private Vector2 startV;
	private Vector2 end;
	private Vector2 endV;	
	private UiController _uiController;
	public PlayerController PlayerControllerInstance;
	
	[Signal]
	public delegate void AreaSelectedEventHandler(Vector2 start, Vector2 end);		
	
	[Signal]
	public delegate void PointSelectedEventHandler(Vector2 point);	
	
	[Signal]
	public delegate void StartMoveSelectionEventHandler();



	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_windowSize = DisplayServer.WindowGetSize();
		_uiController = GetNode<UiController>("../Ui");
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouse mouse) {
			mousePosition = mouse.Position;
			mousePosGlobal = GetGlobalMousePosition();
		}
	}

	
	
	public void DrawArea(bool s = true) {
		var panel = GetNode<Panel>("../Ui/Panel");
		panel.Size = new Vector2(Math.Abs(startV.X - endV.X), Math.Abs(startV.Y - endV.Y));
		var pos = Vector2.Zero;
		pos.X = Math.Min(startV.X, endV.X);
		pos.Y = Math.Min(startV.Y, endV.Y);
		panel.Position = pos;
		panel.Size *= s ? 1 : 0;
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("RMB")) {
			start = GetGlobalMousePosition();
			startV = mousePosition;
			_dragging = false;
			DrawArea(false);
		}
		
		if (PlayerControllerInstance is not null && PlayerControllerInstance.BuildingMode) {
			return;
		}

		if (Input.IsActionJustPressed("LMB")) {
			start = GetGlobalMousePosition();
			startV = mousePosition;
			_dragging = true;
		}

		if (_dragging) {
			end = GetGlobalMousePosition();
			endV = mousePosition;
			DrawArea();
		}

		if (Input.IsActionJustReleased("LMB")) {
			end = GetGlobalMousePosition();
			endV = mousePosition;
			_dragging = false;
			DrawArea(false);
			if (start.DistanceTo(end) > 10) {
				EmitSignal(SignalName.AreaSelected, start, end);
			} else if (!OverUiElement(start)) {
				EmitSignal(SignalName.PointSelected, start);
			}
			
		}

		

		var inpx = 0;
		var inpy = 0;
		
		if (EnableEdgePanning) {
			var b = GetLocalMousePosition();
			if (b.X < 10) {
				inpx = -1;
			} else if (b.X > _windowSize.X) {
				inpx = 1;
			}

			if (b.Y < 10) {
				inpy = -1;
			} else if (b.Y > _windowSize.Y) {
				inpy = 1;
			}
		}

		// var inpx = (Input.IsActionPressed("ui_right") ? 1 : 0) 
		// 		   - (Input.IsActionPressed("ui_left") ? 1 : 0);
		// var inpy = (Input.IsActionPressed("ui_down") ? 1 : 0) 
		// 		   - (Input.IsActionPressed("ui_up") ? 1 : 0);

		Position += new Vector2(inpx * Speed, inpy * Speed);
	}

	private bool OverUiElement(Vector2 position) {
		return _uiController.OverUiElement(position);
	}
}
