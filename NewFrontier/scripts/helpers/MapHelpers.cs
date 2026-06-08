using System;
using Godot;

namespace NewFrontier.scripts.helpers;

public class MapHelpers {
	/// <summary>
	/// Size of one grid cell in world pixels.
	/// Example: grid cell (1, 0) starts at world X = 80 when DrawSize is 80.
	/// </summary>
	public const int DrawSize = 80;

	/// <summary>
	/// Converts a grid-line coordinate to a world-pixel position.
	/// This returns the top-left corner of the cell at that coordinate, not the cell center.
	/// Use this for objects whose position is intentionally aligned to grid intersections or cell corners.
	/// </summary>
	/// <param name="gridLine">Global grid coordinate measured in cells.</param>
	/// <returns>World-pixel position at the grid-line/cell-corner.</returns>
	public static Vector2 GridLineToWorldPoint(Vector2 gridLine) {
		return gridLine * DrawSize;
	}

	/// <summary>
	/// Converts a grid-cell coordinate to the world-pixel position at that cell's center.
	/// Example: grid cell (0, 0) returns (40, 40) when DrawSize is 80.
	/// </summary>
	/// <param name="gridCell">Global grid coordinate of the cell.</param>
	/// <returns>World-pixel position at the center of the cell.</returns>
	public static Vector2 GridCellToWorldCenter(Vector2 gridCell) {
		return (gridCell * DrawSize) + new Vector2(DrawSize / 2f, DrawSize / 2f);
	}

	/// <summary>
	/// Converts a grid-cell coordinate to a random world-pixel position inside that cell's central area.
	/// The generated point is between 25% and 75% of the cell size on both axes, so it avoids cell edges.
	/// </summary>
	/// <param name="gridCell">Global grid coordinate of the cell.</param>
	/// <returns>Random world-pixel position inside the cell.</returns>
	public static Vector2 GridCellToRandomWorldPoint(Vector2 gridCell) {
		var rand = new Random();
		const float halfSize = DrawSize / 2f;
		const float quarterSize = DrawSize / 4f;
		return (gridCell * DrawSize) + new Vector2(quarterSize + (halfSize * rand.NextSingle()),
			quarterSize + (halfSize * rand.NextSingle()));
	}

	/// <summary>
	/// Converts a world-pixel position to the nearest grid-line coordinate.
	/// Adding half a cell before truncating makes positions round to the nearest grid point.
	/// Example with DrawSize 80: world X 39 returns 0, world X 40 returns 1.
	/// </summary>
	/// <param name="worldPoint">World-pixel point.</param>
	/// <returns>Nearest global grid-line coordinate.</returns>
	public static Vector2I WorldPointToNearestGridLine(Vector2 worldPoint) {
		const int halfDrawSize = DrawSize / 2;
		return new Vector2I((int)((worldPoint.X + halfDrawSize) / DrawSize), (int)((worldPoint.Y + halfDrawSize) / DrawSize));
	}

	/// <summary>
	/// Converts a world-pixel position to the grid-cell coordinate containing that position.
	/// This truncates toward zero, so any world position inside cell (0, 0) returns (0, 0).
	/// </summary>
	/// <param name="worldPoint">World-pixel point.</param>
	/// <returns>Global grid-cell coordinate containing the position.</returns>
	public static Vector2I WorldPointToGridCell(Vector2 worldPoint) {
		return new Vector2I((int)worldPoint.X / DrawSize, (int)worldPoint.Y / DrawSize);
	}

	/// <summary>
	/// Converts a sector-local grid coordinate to the global grid coordinate for a sector index.
	/// Sector-local coordinates are measured from the sector's top-left cell.
	/// </summary>
	/// <param name="sectorLocalGrid">Sector-local grid coordinate.</param>
	/// <param name="index">Sector index in the 4 by 4 sector layout.</param>
	/// <returns>Global grid coordinate.</returns>
	public static Vector2I SectorLocalGridToGlobalGrid(Vector2 sectorLocalGrid, int index) {
		return SectorLocalGridToGlobalGrid((int)sectorLocalGrid.X, (int)sectorLocalGrid.Y, index);
	}

	/// <summary>
	/// Converts a sector-local grid coordinate to the global grid coordinate for a sector index.
	/// Sectors are stored as 128 by 128 chunks in a 4 by 4 sector layout.
	/// Example: local (0, 0) in sector index 1 becomes global (128, 0).
	/// </summary>
	/// <param name="col">Sector-local grid column.</param>
	/// <param name="row">Sector-local grid row.</param>
	/// <param name="index">Sector index in the 4 by 4 sector layout.</param>
	/// <returns>Global grid coordinate.</returns>
	public static Vector2I SectorLocalGridToGlobalGrid(int col, int row, int index) {
		const int chunkSize = 128;
		const int rowCount = 4;
		const int colCount = 4;

		var xOffset = index % colCount * chunkSize;
		var yOffset = index / rowCount * chunkSize;

		var finalX = xOffset + col;
		var finalY = yOffset + row;

		return new Vector2I(finalX, finalY);
	}

	/// <summary>
	/// Gets the sector index containing a global grid coordinate.
	/// </summary>
	/// <param name="globalGrid">Global grid coordinate.</param>
	/// <returns>Sector index in the 4 by 4 sector layout.</returns>
	public static byte GlobalGridToSectorIndex(Vector2 globalGrid) {
		return GlobalGridToSectorIndex((int)globalGrid.X, (int)globalGrid.Y);
	}

	/// <summary>
	/// Sector-local coordinate plus the sector index that contains the original global coordinate.
	/// Position is local to the sector's top-left cell.
	/// </summary>
	public readonly record struct SectorGridPosition(Vector2 Position,  int Index);

	/// <summary>
	/// Gets the sector index containing a global grid coordinate.
	/// </summary>
	/// <param name="col">Global grid column.</param>
	/// <param name="row">Global grid row.</param>
	/// <returns>Sector index in the 4 by 4 sector layout.</returns>
	private static byte GlobalGridToSectorIndex(int col, int row) {
		return (byte)GlobalGridToSectorLocalGrid(col, row).Index;
	}

	/// <summary>
	/// Converts a global grid coordinate to a sector-local coordinate and sector index.
	/// This is the inverse of SectorLocalGridToGlobalGrid for valid coordinates.
	/// </summary>
	/// <param name="globalGrid">Global grid coordinate.</param>
	/// <returns>Sector-local coordinate and sector index.</returns>
	public static SectorGridPosition GlobalGridToSectorLocalGrid(Vector2I globalGrid) {
		return GlobalGridToSectorLocalGrid(globalGrid.X, globalGrid.Y);
	}

	/// <summary>
	/// Converts a global grid coordinate to a sector-local coordinate and sector index.
	/// Example: global (130, 5) becomes local (2, 5) in sector index 1.
	/// </summary>
	/// <param name="col">Global grid column.</param>
	/// <param name="row">Global grid row.</param>
	/// <returns>Sector-local coordinate and sector index.</returns>
	public static SectorGridPosition GlobalGridToSectorLocalGrid(int col, int row) {
		const int chunkSize = 128;
		const int chunksPerRow = 4;
		return new SectorGridPosition(
			new Vector2(col % chunkSize, row % chunkSize),
			Index: row / chunkSize * chunksPerRow + col / chunkSize
		);
	}
}
