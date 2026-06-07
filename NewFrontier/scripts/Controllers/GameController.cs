using Godot;
using NewFrontier.scripts.Entities;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Controllers;

public partial class GameController : Node {
	private MapGrid _mapGrid;
	private PackedScene _planetScene;
	private PackedScene _playerControllerScene;

	public override void _Ready() {
		_playerControllerScene = GD.Load<PackedScene>("res://scenes/player_controller.tscn");
		_planetScene = GD.Load<PackedScene>("res://scenes/wet_planet.tscn");

		var player = _playerControllerScene.Instantiate();
		AddChild(player);
		_mapGrid = GetNode<MapGrid>("../MapGrid");
		CreatePlanet(new Vector2(14, 14));
		_mapGrid.CreateWormholes(new Vector2(19, 19), 0, new Vector2(29, 23), 1);
		_mapGrid.CreateWormholes(new Vector2(25, 25), 0, new Vector2(25, 25), 2);
	}

	public override void _Process(double delta) {
	}

	private void CreatePlanet(Vector2 gridCoord) {
		_mapGrid.SetBlocking(gridCoord - new Vector2(2, 2), 4);
		var planet = _planetScene.Instantiate<Planet>();
		planet.Init(gridCoord);
		AddChild(planet);
		_mapGrid.Planets.Add(planet);
	}
}
