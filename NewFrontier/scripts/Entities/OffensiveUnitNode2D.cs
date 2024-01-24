using System;
using Godot;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.Entities;

public partial class OffensiveUnitNode2D : UnitNode2D, IAttack {
	[ExportGroup("OffensiveStats")] [Export]
	public int CurrentSupply;

	[ExportGroup("OffensiveStats")] [Export]
	public int Damage;

	[ExportGroup("OffensiveStats")] [Export]
	public int MaxSupply;

	public IBase Target { get; set; }

	public int CalculateDamage() {
		return Damage;
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if (Target is null) {
			return;
		}

		var d = Target.Instance.Position.DistanceTo(Position);
		GD.Print(d < 200f ? "Pew pew" : $"Distance: {d}");
		((IAttack)this).Attack();
	}
}
