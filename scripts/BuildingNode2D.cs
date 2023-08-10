using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using NewFrontier.scripts;

public partial class BuildingNode2D : Node2D {
	[Export] public int Wide = 1;
	private int baseAngle = 30;
	private Planet _planet;
	private int[] _place = Array.Empty<int>();
	private Sprite2D _sprite;
	private List<Planet> _planets;
	public bool BuildingShade = true;

	private PlayerController _playerController;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_planets = GetTree().GetNodesInGroup("planet").Select(planet => planet as Planet).ToList();
		_sprite = GetNode<Sprite2D>("Sprite2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		// if (this.buildingShade) {
		// 	const npos = this.scene.getWorldPos(ev);
		// 	const distance = Phaser.Math.Distance.Between(npos.x, npos.y, this.scene.planet.x, this.scene.planet.y);
		// 	if (Math.abs(distance - this.scene.planet.radius) < 20) {
		// 		this.buildingShade.unBound = false;
		// 		this.buildingShade.nearPlanet = this.scene.planet;
		// 		const loc = this.buildingShade.calculatePlace(this.scene.planet, npos.x, npos.y);
		// 		console.log("loc", loc);
		// 	} else {
		// 		this.buildingShade.unBound = true;
		// 		this.buildingShade.nearPlanet = null;
		// 		this.buildingShade.setPosition(npos.x, npos.y);
		// 	}
		// }
		if (!BuildingShade) {
			return;
		}
		if (Input.IsActionJustPressed("LMB")) {
			if (_playerController is not null && _planet is not null && _place.Length > 0) {
				_playerController.BuildBuilding(this, _planet, _place);
			}
		}

		var pos = GetGlobalMousePosition();
		if (_planets.Any(planet => DistanceFn(pos, planet))) {
			_planet = _planets.First(planet => DistanceFn(pos, planet));
			_place = CalculatePlace(_planet, pos.X, pos.Y);
			GD.Print(_place.Join());
		} else {
			_planet = null;
			_place = Array.Empty<int>();
			GlobalPosition = pos - (_sprite.Offset * new Vector2(2.5f, 2.5f));
			RotationDegrees = 0;
		}
	}

	public void SetPlayer(PlayerController playerController) {
		_playerController = playerController;
	}
	
	private static bool DistanceFn(Vector2 mousePos, Planet planet) =>
		Math.Abs(mousePos.DistanceTo(planet.GlobalPosition) - planet.Radius) < 20;

	public int[] CalculatePlace(Planet planet, double x, double y) {
		double angle = Math.Atan2(y - planet.GlobalPosition.Y, x - planet.GlobalPosition.X);
		double a = angle * (180 / Math.PI);
		double b = a > 0 ? a : 360 + a;
		int place = (int)((b + (Wide % 2 == 0 ? 0 : 15)) / 30) % 12;
		double posX = planet.GlobalPosition.X;
		double posY = planet.GlobalPosition.Y;
		RotationDegrees = place * 30 - 90 + (Wide % 2 == 0 ? 16 : 0);
		GlobalPosition = new Vector2((float)posX, (float)posY);

		int[] hoverPos;
		switch (Wide) {
			case 1:
				hoverPos = new[] { place };
				break;
			case 2: {
				int prev = place > 11 ? 0 : place;
				int next = place + 1 > 11 ? 0 : place + 1;
				hoverPos = new[] { prev, next };
				break;
			}
			case 3: {
				int prev = place - 1 < 0 ? 11 : place - 1;
				int next = place + 1 > 11 ? 0 : place + 1;
				hoverPos = new[] { prev, place, next };
				break;
			}
			default:
				hoverPos = Array.Empty<int>();
				break;
		}

		return hoverPos;
	}
}
