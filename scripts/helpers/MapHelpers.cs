using System;
using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.helpers;

public class MapHelpers {
	public const int Size = 80;

	public static Vector2 GridCoordToGridPointPos(Vector2 gridCoordinate) => gridCoordinate * Size;
	public static Vector2 GridCoordToGridCenterPos(Vector2 gridCoordinate) => gridCoordinate * Size + new Vector2(Size / 2f, Size / 2f);
	public static Vector2 GridCoordToGridRandomPos(Vector2 gridCoordinate) {
		var rand = new Random();
		return gridCoordinate * Size + 
			new Vector2(Size / 4f + Size / 2f * (float)rand.NextDouble(), 
						Size / 4f + Size / 2f * (float)rand.NextDouble());
	}


	public static Vector2 NodeToPos(GameNode node) => node.Position * Size + new Vector2(Size / 2f, Size / 2f);
	public static Vector2 PosToGrid(Vector2 pos) => new((int)pos.X / Size, (int)pos.Y / Size);
}
