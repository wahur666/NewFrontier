using System;
using System.Collections.Generic;
using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Interfaces;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Entities;

public partial class BuildingNode2D : Node2D, IBase, ISelectable {
	private const int PlanetImgSize = 34;
	private MapGrid _mapGrid;

	private PlayerController _playerController;
	private Sprite2D _sprite;
	private bool _mouseOver;

	public string BuildingName;
	public bool BuildingShade = true;
	public List<object> BuildQueue;
	public List<object> Items;
	public int[] Place = Array.Empty<int>();
	public Planet Planet;
	public List<string> PreRequisites;

	[Export] public SnapOption SnapOption = SnapOption.Planet;

	[Export] public int Wide = 1;
	private bool _selected;

	[Export]
	public bool Selected {
		get => _selected;
		set {
			_selected = value;
			if (SelectionRect is not null) {
				AsISelectable.SetSelection(value);
			}
		}
	}

	[ExportGroup("Stats")] [Export] public int MaxHealth { get; set; }
	[ExportGroup("Stats")] [Export] public int CurrentHealth { get; set; }

	private ISelectable AsISelectable => this;
	public SelectionRect SelectionRect { get; private set; }


	public void Destroy() {
		// Play Anim
		QueueFree();
	}

	public override void _Ready() {
		if (SnapOption == SnapOption.Planet) {
			Scale = new Vector2(Planet.Radius / (float)PlanetImgSize, Planet.Radius / (float)PlanetImgSize);
		}

		_sprite = GetNode<Sprite2D>("Sprite2D");
		SelectionRect = GetNode<SelectionRect>("SelectionRect");
		SelectionRect.MouseEntered += () => _mouseOver = true;
		SelectionRect.MouseExited += () => _mouseOver = false;
	}

	public void Init(PlayerController playerController, string name, int zIndex = 10) {
		_playerController = playerController;
		BuildingName = name;
		ZIndex = zIndex;
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
			1 => [place],
			2 => [place > 11 ? 0 : place, place + 1 > 11 ? 0 : place + 1],
			3 => [place - 1 < 0 ? 11 : place - 1, place, place + 1 > 11 ? 0 : place + 1],
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

	public void CalculateBuildingGridPlace(MapGrid mapGrid, Vector2 pos) {
		if (Wide == 1) {
			var gridPos = MapHelpers.PosToGrid(pos);
			GlobalPosition = MapHelpers.GridCoordToGridCenterPos(gridPos);
		} else if (Wide == 2) {
			var gridPos = MapHelpers.PosToGrid(pos);
			if (gridPos.X % 2 != 1) {
				gridPos += new Vector2I(1, 0);
			}

			if (gridPos.Y % 2 != 1) {
				gridPos += new Vector2I(0, 1);
			}

			GlobalPosition = MapHelpers.GridCoordToGridPointPos(gridPos);
		} else {
			GlobalPosition = pos;
		}
	}

	public void CalculateBuildingWormholePlace(MapGrid mapGrid, Vector2 pos) {
		GlobalPosition = pos;
		var gridPos = MapHelpers.PosToGrid(pos);
		var node = mapGrid.GetGameNode(gridPos);
		if (node is not null && node.HasWormhole) {
			if (gridPos.X % 2 != 1) {
				gridPos += new Vector2I(1, 0);
			}

			if (gridPos.Y % 2 != 1) {
				gridPos += new Vector2I(0, 1);
			}

			GlobalPosition = MapHelpers.GridCoordToGridPointPos(gridPos);
		} else {
			GlobalPosition = pos;
		}
	}
	
	public bool InsideSelectionRect(Vector2 position) {
		return _mouseOver;
	}
}
