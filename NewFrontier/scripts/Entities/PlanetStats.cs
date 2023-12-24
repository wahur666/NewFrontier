using System;
using Godot;

namespace NewFrontier.scripts.Entities;

public partial class PlanetStats : Node {
	[Export] public int MaxOre;
	public int CurrentOre;
	[Export] public int MaxGas;
	public int CurrentGas;
	[Export] public int MaxCrew;
	public int CurrentCrew;

	public override void _Ready() {
		CurrentOre = MaxOre;
		CurrentGas = MaxGas;
		CurrentCrew = MaxCrew;
	}

	public int HarvestOre(int amount) {
		int harvestAmount = Math.Min(CurrentOre, amount);
		CurrentOre -= harvestAmount;
		return harvestAmount;
	}

	public int HarvestGas(int amount) {
		int harvestAmount = Math.Min(CurrentGas, amount);
		CurrentGas -= harvestAmount;
		return harvestAmount;
	}

	public int HarvestCrew(int amount) {
		int harvestAmount = Math.Min(CurrentCrew, amount);
		CurrentCrew -= harvestAmount;
		return harvestAmount;
	}
}
