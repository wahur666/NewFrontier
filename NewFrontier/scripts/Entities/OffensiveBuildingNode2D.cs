using System;
using Godot;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.Entities;

public partial class OffensiveBuildingNode2D : BuildingNode2D, IAttack, ISelectable {
	[ExportGroup("OffensiveStats")] [Export]
	public int CurrentSupply;

	[ExportGroup("OffensiveStats")] [Export]
	public int Damage;

	[ExportGroup("OffensiveStats")] [Export]
	public int MaxSupply;

	public new bool IsUnitSelectable => true;

	public float AttackRange { get; set; } = 400f;
	public IBase Target { get; set; }

	public int CalculateDamage() {
		throw new NotImplementedException();
	}

}
