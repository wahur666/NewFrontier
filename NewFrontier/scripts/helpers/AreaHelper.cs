using System;
using Godot;

namespace NewFrontier.scripts.helpers;

public static class AreaHelper {
	public static bool IsVector2InsideCircle(Vector2 point, Vector2 circleCenter, float circleRadius) {
		float distanceSquared = circleCenter.DistanceSquaredTo(point);
		float radiusSquared = circleRadius * circleRadius;
		return distanceSquared <= radiusSquared;
	}
	
	public static bool InRect(Vector2 point, Vector2 r1, Vector2 r2)
	{
		float x = Math.Min(r1.X, r2.X);
		float y = Math.Min(r1.Y, r2.Y);
		float w = Math.Abs(r1.X - r2.X);
		float h = Math.Abs(r1.Y - r2.Y);
		return x <= point.X && point.X <= x + w && y <= point.Y && point.Y <= y + h;
	}
}
