using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Entities;

public partial class ResourceNode2D : Node2D {
	public ResourceType Resource;
	public int CurrentResource;
	[Export] public int MaxResource = 500;
	[Export] public float BaseScale = 1f;

	public override void _Ready() {
		CurrentResource = MaxResource;
		UpdateScale();
	}

	public void UpdateScale() {
		var newScale = (float)CurrentResource / MaxResource;
		Scale = new Vector2(newScale, newScale) * BaseScale;
	}
}
