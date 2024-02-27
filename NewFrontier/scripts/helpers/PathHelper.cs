using System.Collections.Generic;
using Godot;

namespace NewFrontier.scripts.helpers;

public class PathHelper {
	
	public static List<Vector2> CalculateSmoothPath(List<Vector2> points) {
		var size = points.Count;
		if (size == 0) {
			return [];
		}
		List<Vector2> linePoints = [];
		if (size % 2 == 0 && size > 1) {
			var a = points[size - 1];
			var b = points[size - 2];
			var c = a.Lerp(b, 0.5f);
			points.Insert(size - 1, c);
		}

		linePoints.Add(points[0]);
		var lastM = new Vector2();
		for (int i = 0; i < size - 1; i += 2) {
			var a = points[i];
			var b = points[i + 1];
			var c = points[i + 2];
			var m1 = QuadraticBezier(a, b, c, 0.2f);
			var m2 = QuadraticBezier(a, b, c, 0.4f);
			var m3 = QuadraticBezier(a, b, c, 0.6f);
			var m4 = QuadraticBezier(a, b, c, 0.8f);
			if (i != 0) {
				var e = lastM;
				var f = points[i];
				var g = m1;
				var n1 = QuadraticBezier(e, f, g, 0.33f);
				var n2 = QuadraticBezier(e, f, g, 0.66f);
				linePoints.Add(n1);
				linePoints.Add(n2);
			}
			linePoints.Add(m1);
			linePoints.Add(m2);
			linePoints.Add(m3);
			linePoints.Add(m4);
			lastM = m4;
		}
		linePoints.Add(points[size-1]);
		return linePoints;
	}

	private static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t) {
		var q0 = p0.Lerp(p1, t);
		var q1 = p1.Lerp(p2, t);
		var r = q0.Lerp(q1, t);
		return r;
	}
}
