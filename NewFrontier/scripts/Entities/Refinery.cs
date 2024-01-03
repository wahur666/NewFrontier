using Godot;

namespace NewFrontier.scripts.Entities;

public partial class Refinery : BuildingNode2D {
	
	public Marker2D BuildLocation;

	public override void _Ready() {
		base._Ready();
		BuildLocation = GetNode<Marker2D>("BuildLocation");
	}
}
