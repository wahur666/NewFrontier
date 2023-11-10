using System;
using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.helpers;

public class MapHelpers {
	public const int DrawSize = 80;

	public static Vector2 GridCoordToGridPointPos(Vector2 gridCoordinate) => gridCoordinate * DrawSize;

	public static Vector2 GridCoordToGridCenterPos(Vector2 gridCoordinate) =>
		gridCoordinate * DrawSize + new Vector2(DrawSize / 2f, DrawSize / 2f);

	public static Vector2 GridCoordToGridRandomPos(Vector2 gridCoordinate) {
		var rand = new Random();
		const float halfSize = DrawSize / 2f;
		const float quarterSize = DrawSize / 4f;
		return gridCoordinate * DrawSize + new Vector2(quarterSize + halfSize * rand.NextSingle(),
			quarterSize + halfSize * rand.NextSingle());
	}


	public static Vector2 NodeToPos(GameNode node) => node.Position * DrawSize + new Vector2(DrawSize / 2f, DrawSize / 2f);
	public static Vector2I PosToGrid(Vector2 pos) => new((int)pos.X / DrawSize, (int)pos.Y / DrawSize);

	public static Vector2I CalculateOffset(Vector2 pos, int index) => CalculateOffset((int)pos.X, (int)pos.Y, index);

	public static Vector2I CalculateOffset(int col, int row, int index) {
		const int chunkSize = 128;
		const int rowCount = 4;
		const int colCount = 4;

		// Calculate the offset based on the index
		int xOffset = (index % colCount) * chunkSize;
		int yOffset = (index / rowCount) * chunkSize;

		// Calculate the final position by adding the row and column offsets
		int finalX = xOffset + col;
		int finalY = yOffset + row;

		return new(finalX, finalY);
	}

	public static void GetPositionFromOffset(int col, int row, out int col2, out int row2, out int index) {
		const int chunkSize = 128;
		const int colCount = 4;

		int ocol = col % chunkSize;
		int orow = row % chunkSize;

		int xOffset = col - ocol;
		int yOffset = row - orow;

		int colIndex = xOffset / chunkSize;
		int rowIndex = yOffset / chunkSize;

		// Calculate the final index
		index = rowIndex * colCount + colIndex;

		row2 = orow;
		col2 = ocol;
	}
}
