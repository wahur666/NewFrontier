using Godot;
using System;

namespace NewFrontier.scripts;

public partial class UnitNode2D : CharacterBody2D {
	[Export]
	public virtual bool Selected { get; set; }
}
