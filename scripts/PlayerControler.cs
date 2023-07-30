using Godot;
using System;

public partial class PlayerControler : Node {

	private int _maxCrew = 2500;
	private int _maxOre = 5500;
	private int _maxGas = 4500;
	private int _maxSupply = 0;

	private int _currentCrew;
	private int _currentOre;
	private int _currentGas;
	private int _currentSupply;

	private Variant[] buildings;
	private Variant[] units;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}
}
