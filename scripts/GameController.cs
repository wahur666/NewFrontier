using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts;

public partial class GameController : Node {

	private PackedScene _playerControllerScene;
	private CameraController _camera;
	private List<UnitNode2D> _units;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_playerControllerScene = GD.Load<PackedScene>("res://scenes/player_controller.tscn");
		var player = _playerControllerScene.Instantiate();
		AddChild(player);
		_camera = GetNode<CameraController>("../Camera2D");
		_camera.AreaSelected += SelectUnitsInArea;
		_camera.PointSelected += SelectUnitNearPoint;
		_units = GetNode("../Units").GetChildren().Select(x => x as UnitNode2D).ToList();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public void SelectUnitsInArea(Vector2 start, Vector2 end) {
		foreach (var unit in _units) {
			unit.Selected = AreaHelper.InRect(unit.Position, start, end);
		}
	}

	public void SelectUnitNearPoint(Vector2 point) {
		_units.ForEach(x => x.Selected = false);
		// TODO refactor to only include unit if its inside its selected area
		var unitNode2D = _units.Find(x => x.Position.DistanceTo(point) < 30); 
		if (unitNode2D is not null) {
			unitNode2D.Selected = true;
		}
	}
	
}
