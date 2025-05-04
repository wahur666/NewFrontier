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
		_mapGrid.SetBlocking(new Vector2(12, 12), 2);
		CreatePlanet(new Vector2(13, 13));
		_mapGrid.CreateWormholes(new Vector2(19, 19), 0, new Vector2(14, 15), 1);
		_mapGrid.CreateWormholes(new Vector2(17, 13), 0, new Vector2(6, 17), 2);
	}

	public override void _Process(double delta) {
	}

	private void CreatePlanet(Vector2 gridCoord) {
		var planet = _planetScene.Instantiate<Planet>();
		planet.Position = MapHelpers.GridCoordToGridPointPos(gridCoord);
		AddChild(planet);
		_mapGrid.Planets.Add(planet);
	}
}
