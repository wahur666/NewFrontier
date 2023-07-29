using Godot;
using System;

namespace NewFrontier.scripts;

public partial class PlanetBuildingScheme : Node2D {
	[Export] public int Radius = 83;
	[Export] public int Width = 8;
	[Export] public Color SchemeColor = Color.FromHtml("#87cefa");

	private readonly float _slice = Mathf.DegToRad(30);
	// private readonly float _sliceOffset = Mathf.DegToRad(2.5f);
	private readonly float _sliceOffset = Mathf.DegToRad(15 + 2f);
	private readonly float _sliceLength = Mathf.DegToRad(25);


	public override void _Draw() {
		for (int i = 0; i < 12; i++) {
			DrawArc(new Vector2(0, 0), Radius, i * _slice + _sliceOffset, i * _slice + _sliceOffset + _sliceLength, 32,
				SchemeColor, Width);
		}
	}
}
