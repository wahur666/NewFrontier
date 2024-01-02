using Godot;
using NewFrontier.scripts.Entities;

namespace NewFrontier.scripts.Model.Interfaces;

public interface IBuildable {
	public Node2D Instance { get; }
	public int Wide { get; }
	public SnapOption SnapOption { get; }
	public int[] Place { get; set; }

	public Planet Planet { get; set; }
	public string BuildingName { get; }
	public Sprite2D BuildingSprite { get; }
}
