using System;
using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.helpers;

public class MapHelpers {
	public const int DrawSize = 80;

	public static Vector2 GridCoordToGridPointPos(Vector2 gridCoordinate) {
		return gridCoordinate * DrawSize;
	}

	public static Vector2 GridCoordToGridCenterPos(Vector2 gridCoordinate) {
		return (gridCoordinate * DrawSize) + new Vector2(DrawSize / 2f, DrawSize / 2f);
	}

	public static Vector2 GridCoordToGridRandomPos(Vector2 gridCoordinate) {
		var rand = new Random();
		const float halfSize = DrawSize / 2f;
		const float quarterSize = DrawSize / 4f;
		return (gridCoordinate * DrawSize) + new Vector2(quarterSize + (halfSize * rand.NextSingle()),
			quarterSize + (halfSize * rand.NextSingle()));
	}


	public static Vector2 NodeToPos(GameNode node) {
		return (node.Position * DrawSize) + new Vector2(DrawSize / 2f, DrawSize / 2f);
	}

	public static Vector2I PosToGrid(Vector2 pos) {
		return new Vector2I((int)pos.X / DrawSize, (int)pos.Y / DrawSize);
	}

	public static Vector2I CalculateOffset(Vector2 pos, int index) {
		return CalculateOffset((int)pos.X, (int)pos.Y, index);
	}

	public static Vector2I CalculateOffset(int col, int row, int index) {
		const int chunkSize = 128;
		const int rowCount = 4;
		const int colCount = 4;

		// Calculate the offset based on the index
		var xOffset = index % colCount * chunkSize;
		var yOffset = index / rowCount * chunkSize;

		// Calculate the final position by adding the row and column offsets
		var finalX = xOffset + col;
		var finalY = yOffset + row;

		return new Vector2I(finalX, finalY);
	}
	public static byte GetSectorIndexFromOffset(Vector2 pos) => GetSectorIndexFromOffset((int)pos.X, (int)pos.Y);

	private static byte GetSectorIndexFromOffset(int col, int row) {
		GetPositionFromOffset(col, row, out _, out _, out var index);
		return (byte)index;
	}

	public static void GetPositionFromOffset(int col, int row, out int col2, out int row2, out int index) {
		const int chunkSize = 128;
		const int colCount = 4;

		var ocol = col % chunkSize;
		var orow = row % chunkSize;

		var xOffset = col - ocol;
		var yOffset = row - orow;

		var colIndex = xOffset / chunkSize;
		var rowIndex = yOffset / chunkSize;

		// Calculate the final index
		index = (rowIndex * colCount) + colIndex;

		row2 = orow;
		col2 = ocol;
	}
}
