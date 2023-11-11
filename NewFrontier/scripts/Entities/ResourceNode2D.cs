using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Entities;

public partial class ResourceNode2D : Node2D {
	[Export] public float BaseScale = 1f;
	public int CurrentResource;
	[Export] public int MaxResource = 500;
	public ResourceType Resource;

	public override void _Ready() {
		CurrentResource = MaxResource;
		UpdateScale();
	}

	public void UpdateScale() {
		var newScale = (float)CurrentResource / MaxResource;
		Scale = new Vector2(newScale, newScale) * BaseScale;
	}
}
