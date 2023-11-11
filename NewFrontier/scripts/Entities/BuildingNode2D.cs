using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Controllers;

namespace NewFrontier.scripts.Entities;

public partial class BuildingNode2D : Node2D {
	private int[] _place = Array.Empty<int>();
	private Planet _planet;
	private List<Planet> _planets;

	private PlayerController _playerController;
	private Sprite2D _sprite;
	private int baseAngle = 30;
	public bool BuildingShade = true;

	public int ImgSize = 34;
	[Export] public int Wide = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_planets = GetTree().GetNodesInGroup("planet").Select(planet => planet as Planet).ToList();
		Scale = new Vector2(Planet.Radius / (float)ImgSize, Planet.Radius / (float)ImgSize);
		GD.Print("Scale", Scale);
		_sprite = GetNode<Sprite2D>("Sprite2D");
	}

	public void Init(PlayerController playerController, int zIndex = 10) {
		_playerController = playerController;
		ZIndex = zIndex;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
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
			_place = CalculatePlace(_planet, pos);
		} else {
			_planet = null;
			_place = Array.Empty<int>();
			GlobalPosition = pos - (_sprite.Offset * Scale);
			RotationDegrees = 0;
		}
	}

	private static bool DistanceFn(Vector2 mousePos, Planet planet) {
		return Math.Abs(mousePos.DistanceTo(planet.GlobalPosition) - Planet.Radius) < 20;
	}

	private int[] CalculatePlace(Node2D planet, Vector2 pos) {
		double angle = Mathf.RadToDeg(planet.GlobalPosition.AngleToPoint(pos));
		var b = angle > 0 ? angle : 360 + angle;
		var place = (int)((b + (Wide % 2 == 0 ? 0 : 15)) / 30) % 12;
		RotationDegrees = (place * 30) - 90 + (Wide % 2 == 0 ? 16 : 0);
		GlobalPosition = planet.GlobalPosition;
		return Wide switch {
			1 => new[] { place },
			2 => new[] { place > 11 ? 0 : place, place + 1 > 11 ? 0 : place + 1 },
			3 => new[] { place - 1 < 0 ? 11 : place - 1, place, place + 1 > 11 ? 0 : place + 1 },
			_ => Array.Empty<int>()
		};
	}
}
