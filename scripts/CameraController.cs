using Godot;
using System;


namespace NewFrontier.scripts;

public partial class CameraController : Camera2D {

	[Export] public float Speed = 10.0f;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {

		var a = GetGlobalMousePosition();
		var b = GetLocalMousePosition();
		GD.Print(a + "" + b);


		var inpx = 0;
		var inpy = 0;

		inpx = b.X switch {
			< 10 => -1,
			> 1142 => 1,
			_ => inpx
		};

		inpy = b.Y switch {
			< 10 => -1,
			> 638 => 1,
			_ => inpy
		};

		// var inpx = (Input.IsActionPressed("ui_right") ? 1 : 0) 
		// 		   - (Input.IsActionPressed("ui_left") ? 1 : 0);
		// var inpy = (Input.IsActionPressed("ui_down") ? 1 : 0) 
		// 		   - (Input.IsActionPressed("ui_up") ? 1 : 0);

		Position += new Vector2(inpx * Speed, inpy * Speed);
	}
}
