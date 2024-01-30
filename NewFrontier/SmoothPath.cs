using System.Linq;
using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier;

public partial class SmoothPath : Node2D {
	public Line2D Line;
	public Path2D Path;

	public override void _Ready() {
		Line = GetNode<Line2D>("Line2D");
		Path = GetNode<Path2D>("Path2D");

		Line.ClearPoints();
		var points = Path.Curve.Tessellate().ToList();
		PathHelper.CalculateSmoothPath(points).ForEach(point => Line.AddPoint(point));
	}

}
