using Godot;
using NewFrontier.scripts.Entities;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;
using System;

namespace NewFrontier.scripts.Controllers;

public partial class GameController : Node {

	private PackedScene _playerControllerScene;
	private PackedScene _planetScene;
	private MapGrid _mapGrid;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_playerControllerScene = GD.Load<PackedScene>("res://scenes/player_controller.tscn");
		var player = _playerControllerScene.Instantiate();
		AddChild(player);
		_planetScene = GD.Load<PackedScene>("res://scenes/wet_planet.tscn");
		var planet2 = _planetScene.Instantiate<Planet>();
		planet2.Position = MapHelpers.GridCoordToGridPointPos(new Vector2(15, 15));
		AddChild(planet2);
		_mapGrid = GetNode<MapGrid>("../MapGrid");
		_mapGrid.SetBlocking(new Vector2(13, 13), 4);
		GD.Print("MapGrid: ", _mapGrid);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	
	
}
