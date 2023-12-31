using Godot;

namespace NewFrontier.scripts.UI;

public partial class SelectionRect : TextureRect {
	[Export] public bool Selected;

	public override void _Draw() {
		if (!Selected) {
			return;
		}

		DrawColoredPolygon([
			Vector2.Zero,
			new Vector2((Size.X / 2) - (int)(Size.X * .25), 0),
			new Vector2(0, (Size.X / 2) - (int)(Size.X * .25))
		], Colors.White);
		DrawColoredPolygon([
			new Vector2((Size.X / 2) + (int)(Size.X * .25), 0),
			new Vector2(Size.X, 0),
			new Vector2(Size.X, (Size.X / 2) - (int)(Size.X * .25))
		], Colors.White);
		DrawColoredPolygon([
			new Vector2((Size.X / 2) - (int)(Size.X * .25), Size.Y),
			new Vector2(0, Size.Y),
			new Vector2(0, Size.Y - (int)(Size.X * .25))
		], Colors.White);
		DrawColoredPolygon([
			new Vector2((Size.X / 2) - (int)(Size.X * .25), Size.Y),
			new Vector2(0, Size.Y),
			new Vector2(0, Size.Y - (int)(Size.X * .25))
		], Colors.Azure);
		DrawColoredPolygon([
			new Vector2(Size.X, Size.Y),
			new Vector2((Size.X / 2) + (int)(Size.X * .25), Size.Y),
			new Vector2(Size.X, Size.Y - (int)(Size.X * .25))
		], Colors.White);
	}

	public override void _Process(double delta) {
		QueueRedraw();
	}
}
