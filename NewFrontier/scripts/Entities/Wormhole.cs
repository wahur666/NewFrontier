using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Entities;

public partial class Wormhole : Node2D {
	private Area2D _area2D;
	public BuildingNode2D Building;
	public GameNode GameNode;
	public bool MousePointerIsOver;
	private Node _container;

	public override void _Ready() {
		_area2D = GetNode<Area2D>("Area2D");
		_area2D.MouseEntered += Area2DOnMouseEntered;
		_area2D.MouseExited += Area2DOnMouseExited;
		_container = GetNode<Node>("Container");
	}

	public void Init(GameNode gameNode) {
		GameNode = gameNode;
	}

	private void Area2DOnMouseExited() {
		MousePointerIsOver = false;
	}

	private void Area2DOnMouseEntered() {
		MousePointerIsOver = true;
	}

	public BuildingNode2D Build(BuildingNode2D buildingNode2D) {
		Building = buildingNode2D.Duplicate() as BuildingNode2D;
		Building.BuildingShade = false;
		Building.GlobalPosition = GlobalPosition;
		_container.AddChild(Building);
		return Building;
	}

	private void RemoveBuilding() {
		RemoveChild(Building);
		Building.QueueFree();
	}
}
