using Godot;
using NewFrontier.scripts.Controllers;

namespace NewFrontier.scripts.Entities;

public partial class UnitIcon : Button {
	public PlayerController PlayerController;
	public UnitNode2D Unit;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Pressed += SelectUnit;
	}

	private void SelectUnit() {
		PlayerController.SelectUnit(Unit);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}
}
