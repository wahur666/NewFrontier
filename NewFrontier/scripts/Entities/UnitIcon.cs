using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.Entities;

public partial class UnitIcon : Button {
	public PlayerController PlayerController;
	public ISelectable Unit;
	public TextureRect TextureRect;

	public override void _Ready() {
		Pressed += SelectUnit;
		TextureRect = GetNode<TextureRect>("TextureRect");
	}

	private void SelectUnit() {
		PlayerController.SelectUnitFromUi(Unit);
	}

	public override void _Process(double delta) {
	}
}
