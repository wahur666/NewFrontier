using System;
using Godot;

namespace NewFrontier.scripts.helpers;

public static class AreaHelper {
	public static bool IsVector2InsideCircle(Vector2 point, Vector2 circleCenter, float circleRadius) {
		var distanceSquared = circleCenter.DistanceSquaredTo(point);
		var radiusSquared = circleRadius * circleRadius;
		return distanceSquared <= radiusSquared;
	}

	public static bool InRect(Vector2 point, Vector2 r1, Vector2 r2) {
		var x = Math.Min(r1.X, r2.X);
		var y = Math.Min(r1.Y, r2.Y);
		var w = Math.Abs(r1.X - r2.X);
		var h = Math.Abs(r1.Y - r2.Y);
		return x <= point.X && point.X <= x + w && y <= point.Y && point.Y <= y + h;
	}
}
