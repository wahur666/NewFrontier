using Godot;
using NewFrontier.scripts.Controllers;

namespace NewFrontier.scripts.Entities;

public partial class UnitIcon : Button {
	public PlayerController PlayerController;
	public UnitNode2D Unit;

	public override void _Ready() {
		Pressed += SelectUnit;
	}

	private void SelectUnit() {
		PlayerController.SelectUnitFromUi(Unit);
	}

	public override void _Process(double delta) {
	}
}
