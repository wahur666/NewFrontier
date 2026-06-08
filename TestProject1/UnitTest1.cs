using Godot;
using NewFrontier.scripts.helpers;

namespace TestProject1;

public class Tests {
	[SetUp]
	public void Setup() {
	}

	[Test]
	public void TestCalculateOffset() {
		var pos = MapHelpers.CalculateOffset(9, 15, 1);
		var result = new Vector2I(137, 15);
		Assert.That(result, Is.EqualTo(pos));

		var pos2 = MapHelpers.CalculateOffset(15, 20, 4);
		var result2 = new Vector2I(15, 148);
		Assert.That(result2, Is.EqualTo(pos2));
	}

	[Test]
	public void TestCalculateOffset2() {
		var pos = MapHelpers.CalculateOffset(new Vector2(9, 15), 1);
		var pos2 = MapHelpers.CalculateOffset(9, 15, 1);
		Assert.That(pos, Is.EqualTo(pos2));
	}

	[Test]
	public void TestGetPositionFromOffset() {
		var a = new Vector2I(143, 15);

		var res = MapHelpers.GetPositionFromOffset(a.X, a.Y);
		Assert.Multiple(() => {
			Assert.That(res.Col, Is.EqualTo(15));
			Assert.That(res.Row, Is.EqualTo(15));
			Assert.That(res.Index, Is.EqualTo(1));
		});
		var b = MapHelpers.CalculateOffset(34, 52, 7);
		res = MapHelpers.GetPositionFromOffset(b.X, b.Y);
		Assert.Multiple(() => {
			Assert.That(res.Col, Is.EqualTo(34));
			Assert.That(res.Row, Is.EqualTo(52));
			Assert.That(res.Index, Is.EqualTo(7));
		});
	}
}
