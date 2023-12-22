using System;
using Godot;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.Entities;

public partial class OffensiveBuildingNode2D : BuildingNode2D, IAttack {
	[ExportGroup("OffensiveStats")] [Export]
	public int CurrentSupply;

	[ExportGroup("OffensiveStats")] [Export]
	public int Damage;

	[ExportGroup("OffensiveStats")] [Export]
	public int MaxSupply;

	public int CalculateDamage(IBase target) {
		throw new NotImplementedException();
	}

	public void Attack(IBase target) {
		throw new NotImplementedException();
	}
}
