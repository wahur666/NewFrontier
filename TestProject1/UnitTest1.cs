using Godot;
using NewFrontier.scripts.helpers;

namespace TestProject1;

public class Tests
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Test1()
	{
		var pos = MapHelpers.CalculateOffset(15, 15, 1);
		var result = new Vector2I(143, 15);
		Assert.That(result, Is.EqualTo(pos));
		
		var pos2 = MapHelpers.CalculateOffset(15, 15, 4);
		var result2 = new Vector2I(15, 143);
		Assert.That(result2, Is.EqualTo(pos2));
	}
}