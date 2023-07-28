using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts; 

public partial class ResourceNode2D : Node2D {

	public ResourceType Resource;
	public int CurrentResource;
	[Export] public int MaxResource = 500;

	public void UpdateScale() {
		var newScale = (float)CurrentResource / MaxResource;
		Scale = new Vector2(newScale, newScale);
	}

}
