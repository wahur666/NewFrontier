using Godot;

namespace NewFrontier.scripts.Entities;

public partial class PlanetBuildingScheme : Node2D {

	private readonly float _sliceLength = Mathf.DegToRad(25);

	// private readonly float _sliceOffset = Mathf.DegToRad(2.5f);
	public static readonly float Slice = Mathf.DegToRad(30);
	public static readonly float SliceOffset = Mathf.DegToRad(15 + 2f);
	public int Radius;
	[Export] public Color SchemeColor = Color.FromHtml("#87cefa");
	[Export] public int Width = 8;


	public override void _Draw() {
		for (var i = 0; i < 12; i++) {
			DrawArc(new Vector2(0, 0), Radius, (i * Slice) + SliceOffset, (i * Slice) + SliceOffset + _sliceLength,
				32,
				SchemeColor, Width);
		}
	}
}
