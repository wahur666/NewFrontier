using Godot;
using System;

public partial class PlayerControler : Node {

	public int MaxCrew = 2500;
	public int MaxOre = 5500;
	public int MaxGas = 4500;
	public int MaxSupply = 0;

	public int CurrentCrew = 0;
	[Export]public int CurrentOre = 0;
	[Export]public int CurrentGas = 0;
	public int CurrentSupply = 0;

	private Variant[] buildings;
	private Variant[] units;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public int AvailableOreStorage() => MaxOre - CurrentOre;
	public int AvailableGasStorage() => MaxGas - CurrentGas;
	public int AvailableCrewStorage() => MaxCrew - CurrentCrew;
}
