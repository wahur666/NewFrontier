using Godot;

namespace NewFrontier.scripts.helpers;

public static class CircleHelper {
	public static bool IsVector2InsideCircle(Vector2 point, Vector2 circleCenter, float circleRadius) {
		float distanceSquared = circleCenter.DistanceSquaredTo(point);
		float radiusSquared = circleRadius * circleRadius;
		return distanceSquared <= radiusSquared;
	}
}
