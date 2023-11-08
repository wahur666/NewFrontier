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
	public void TestGetPositionFromOffset() {
		var a = new Vector2I(143, 15);
		
		MapHelpers.GetPositionFromOffset(a.X, a.Y, out var col, out var row , out var index);
		Assert.Multiple(() => {
			Assert.That(col, Is.EqualTo(15));
			Assert.That(row, Is.EqualTo(15));
			Assert.That(index, Is.EqualTo(1));
		});
		var b = MapHelpers.CalculateOffset(34, 52, 7);
		MapHelpers.GetPositionFromOffset(b.X, b.Y, out var col2, out var row2, out var index2);
		Assert.Multiple(() => {
			Assert.That(col2, Is.EqualTo(34));
			Assert.That(row2, Is.EqualTo(52));
			Assert.That(index2, Is.EqualTo(7));
		});
	}
}
