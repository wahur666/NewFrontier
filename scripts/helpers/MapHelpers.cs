using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.helpers; 

public class MapHelpers {
	public static Vector2 GridCoordToGridPointPos(Vector2 gridCoordinate, float size) => gridCoordinate * size;
	public static Vector2 GridCoordToGridCenterPos(Vector2 gridCoordinate, float size) => gridCoordinate * size + new Vector2(size / 2, size / 2);
	public static Vector2 NodeToPos(GameNode node, float size) => node.Position * size + new Vector2(size / 2, size / 2);
	public static Vector2 PosToGrid(Vector2 pos, float size) => new((int)pos.X / size, (int)pos.Y / size);
}
