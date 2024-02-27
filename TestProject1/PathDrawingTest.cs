namespace TestProject1;

public class PathDrawingTest {
	
	[SetUp]
	public void Setup() {
	}


	[Test]
	public void CheckPoints() {
		// (12, 17), (12, 16), (11, 15), (11, 14), (11, 13), (11, 12), (12, 11)
		var start = new Point(12, 17);
		var end = new Point(12, 11);
		var a = GetPointsInBetween(start, end);
		var b = String.Join(", ", a.Select(z => (z.X, z.Y)));
		var c = String.Join(", ", new List<Point> {
			new(12, 17), 
			new(12, 16), 
			new(12, 15), 
			new(12, 14), 
			new(12, 13), 
			new(12, 12), 
			new(12, 11)
		}.Select(z => (z.X, z.Y)));
		Assert.That(c, Is.EqualTo(b));
	}
	
	static List<Point> GetPointsInBetween(Point start, Point end) {
		List<Point> pointsInBetween = new List<Point>();

		int dx = Math.Abs(end.X - start.X);
		int dy = Math.Abs(end.Y - start.Y);
		int sx = (start.X < end.X) ? 1 : -1;
		int sy = (start.Y < end.Y) ? 1 : -1;

		int err = dx - dy;

		int currentX = start.X;
		int currentY = start.Y;

		while (true) {
			pointsInBetween.Add(new Point(currentX, currentY));

			if (currentX == end.X && currentY == end.Y) {
				break; // Reached the end
			}

			int e2 = 2 * err;
			if (e2 > -dy) {
				err -= dy;
				currentX += sx;
			}

			if (e2 < dx) {
				err += dx;
				currentY += sy;
			}
		}

		return pointsInBetween;
	}

	class Point {
		public int X { get; set; }
		public int Y { get; set; }

		public Point(int x, int y) {
			X = x;
			Y = y;
		}
	}
}
