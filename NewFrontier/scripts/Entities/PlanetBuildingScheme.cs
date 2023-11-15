using Godot;

namespace NewFrontier.scripts.Entities;

public partial class PlanetBuildingScheme : Node2D {
	private readonly float _slice = Mathf.DegToRad(30);

	private readonly float _sliceLength = Mathf.DegToRad(25);

	// private readonly float _sliceOffset = Mathf.DegToRad(2.5f);
	private readonly float _sliceOffset = Mathf.DegToRad(15 + 2f);
	public int Radius;
	[Export] public Color SchemeColor = Color.FromHtml("#87cefa");
	[Export] public int Width = 8;


	public override void _Draw() {
		for (var i = 0; i < 12; i++) {
			DrawArc(new Vector2(0, 0), Radius, (i * _slice) + _sliceOffset, (i * _slice) + _sliceOffset + _sliceLength,
				32,
				SchemeColor, Width);
		}
	}
}
