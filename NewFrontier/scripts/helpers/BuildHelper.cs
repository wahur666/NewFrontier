using System;
using Godot;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.helpers;

public static class BuildHelper {
	public static void CalculateBuildingPlace(IBuildable buildable, MapGrid mapGrid, Vector2 pos) {
		if (buildable.SnapOption == SnapOption.Planet) {
			CalculateBuildingPlanetPlace(buildable, mapGrid, pos);
		} else if (buildable.SnapOption == SnapOption.Grid) {
			CalculateBuildingGridPlace(buildable, mapGrid, pos);
		} else {
			CalculateBuildingWormholePlace(buildable, mapGrid, pos);
		}
	}


	private static int[] CalculatePlaceOnPlanet(IBuildable buildable, Node2D planet, Vector2 pos) {
		const int planetAngleSize = 30;
		const int planetHalfAngleSize = 15;
		double angle = Mathf.RadToDeg(planet.GlobalPosition.AngleToPoint(pos));
		var b = angle > 0 ? angle : 360 + angle;
		var place = (int)((b + (buildable.Wide % 2 == 0 ? 0 : planetHalfAngleSize)) / planetAngleSize) % 12;
		buildable.Instance.RotationDegrees =
			(place * planetAngleSize) - 90 + (buildable.Wide % 2 == 0 ? planetHalfAngleSize : 0);
		buildable.Instance.GlobalPosition = planet.GlobalPosition;
		return buildable.Wide switch {
			1 => [place],
			2 => [place > 11 ? 0 : place, place + 1 > 11 ? 0 : place + 1],
			3 => [place - 1 < 0 ? 11 : place - 1, place, place + 1 > 11 ? 0 : place + 1],
			_ => Array.Empty<int>()
		};
	}


	private static void CalculateBuildingPlanetPlace(IBuildable buildable, MapGrid mapGrid, Vector2 pos) {
		buildable.Planet = mapGrid.Planets.Find(planet => planet.PointNearToRing(pos));
		if (buildable.Planet is not null) {
			buildable.Place = CalculatePlaceOnPlanet(buildable, buildable.Planet, pos);
		} else {
			buildable.Place = Array.Empty<int>();
			buildable.Instance.GlobalPosition = pos - (buildable.BuildingSprite.Offset * buildable.Instance.Scale);
			buildable.Instance.RotationDegrees = 0;
		}
	}

	private static void CalculateBuildingGridPlace(IBuildable buildable, MapGrid mapGrid, Vector2 pos) {
		if (buildable.Wide == 1) {
			var gridPos = MapHelpers.PosToGrid(pos);
			buildable.Instance.GlobalPosition = MapHelpers.GridCoordToGridCenterPos(gridPos);
		} else if (buildable.Wide == 2) {
			var gridPos = MapHelpers.PosToGrid(pos);
			if (gridPos.X % 2 != 1) {
				gridPos += new Vector2I(1, 0);
			}

			if (gridPos.Y % 2 != 1) {
				gridPos += new Vector2I(0, 1);
			}

			buildable.Instance.GlobalPosition = MapHelpers.GridCoordToGridPointPos(gridPos);
		} else {
			buildable.Instance.GlobalPosition = pos;
		}
	}

	private static void CalculateBuildingWormholePlace(IBuildable buildable, MapGrid mapGrid, Vector2 pos) {
		buildable.Instance.GlobalPosition = pos;
		var gridPos = MapHelpers.PosToGrid(pos);
		var node = mapGrid.GetGameNode(gridPos);
		if (node is not null && node.HasWormhole) {
			if (gridPos.X % 2 != 1) {
				gridPos += new Vector2I(1, 0);
			}

			if (gridPos.Y % 2 != 1) {
				gridPos += new Vector2I(0, 1);
			}

			buildable.Instance.GlobalPosition = MapHelpers.GridCoordToGridPointPos(gridPos);
		} else {
			buildable.Instance.GlobalPosition = pos;
		}
	}
}
