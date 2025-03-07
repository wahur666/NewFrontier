using Godot;

namespace NewFrontier.scripts;

public partial class Level : Node2D {
	public override void _Ready() {
		// Input.MouseMode = Input.MouseModeEnum.Confined;
	}

	public override void _Process(double delta) {
		if (Input.IsActionJustReleased("ui_cancel")) {
			GetTree().Quit();
		}

		if (Input.IsActionJustPressed("pause-resume")) {
			GD.Print("Pressed");
			GetTree().Paused = !GetTree().Paused;
		}
	}
}
