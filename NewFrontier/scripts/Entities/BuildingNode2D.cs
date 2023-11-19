using System;
using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Entities;

public partial class BuildingNode2D : Node2D {
	public int[] Place = Array.Empty<int>();
	public Planet Planet;

	private PlayerController _playerController;
	private Sprite2D _sprite;
	public bool BuildingShade = true;

	private const int ImgSize = 34;
	[Export] public int Wide = 1;

	public string BuildingName;

	/// <summary>
	///		Snap to planet or snap to grid
	/// </summary>
	public SnapOption SnapToPlanet = SnapOption.Planet;

	public override void _Ready() {
		if (SnapToPlanet == SnapOption.Planet) {
			Scale = new Vector2(Planet.Radius / (float)ImgSize, Planet.Radius / (float)ImgSize);
		}
		_sprite = GetNode<Sprite2D>("Sprite2D");
	}

	public BuildingNode2D Init(PlayerController playerController, string name, int zIndex = 10) {
		_playerController = playerController;
		BuildingName = name;
		ZIndex = zIndex;
		return this;
	}

	private int[] CalculatePlaceOnPlanet(Node2D planet, Vector2 pos) {
		const int planetAngleSize = 30;
		const int planetHalfAngleSize = 15;
		double angle = Mathf.RadToDeg(planet.GlobalPosition.AngleToPoint(pos));
		var b = angle > 0 ? angle : 360 + angle;
		var place = (int)((b + (Wide % 2 == 0 ? 0 : planetHalfAngleSize)) / planetAngleSize) % 12;
		RotationDegrees = (place * planetAngleSize) - 90 + (Wide % 2 == 0 ? planetHalfAngleSize : 0);
		GlobalPosition = planet.GlobalPosition;
		return Wide switch {
			1 => new[] { place },
			2 => new[] { place > 11 ? 0 : place, place + 1 > 11 ? 0 : place + 1 },
			3 => new[] { place - 1 < 0 ? 11 : place - 1, place, place + 1 > 11 ? 0 : place + 1 },
			_ => Array.Empty<int>()
		};
	}

	public void CalculateBuildingPlace(Vector2 pos, Planet planet = null) {
		Planet = planet;
		if (Planet is not null) {
			Place = CalculatePlaceOnPlanet(planet, pos);
		} else {
			Place = Array.Empty<int>();
			GlobalPosition = pos - (_sprite.Offset * Scale);
			RotationDegrees = 0;
		}
	}

	public void CalculateBuildingGridPlace(Vector2 pos) {
		if (Wide == 1) {
			var gridPos = MapHelpers.PosToGrid(pos);
			GlobalPosition = MapHelpers.GridCoordToGridCenterPos(gridPos);
		} else if (Wide == 2) {
			var gridPos = MapHelpers.PosToGrid(pos);
			GlobalPosition = MapHelpers.GridCoordToGridPointPos(gridPos);
		} else {
			GlobalPosition = pos;
		}
	}
}
