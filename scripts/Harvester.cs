using System;
using Godot;

namespace NewFrontier.scripts; 

public partial class Harvester : Node2D {

	[Export] public int MaxCargo = 500;
	private int _currentGasCargo = 0;
	private int _currentOreCargo = 0;
	[Export] public float HarvestRate = 0.2f;
	private float _currentHarvest;
	[Export] public int HarvestAmount = 25;
	
	private bool _harvesting = false;

	private ResourceNode2D _currentResourceNode2D = null;

	public override void _Ready() {
		_currentHarvest = HarvestRate;
	}

	public void Harvest(ResourceNode2D resourceNode2D, int amount) {
		var transferAmount = Math.Min(Math.Min((MaxCargo - (_currentGasCargo + _currentOreCargo)), resourceNode2D.CurrentResource), amount);
		resourceNode2D.CurrentResource -= transferAmount;
		_currentGasCargo += transferAmount;
	}

	public override void _Process(double delta) {
		if (_currentResourceNode2D is not null && _harvesting) {
			if (_currentHarvest == 0) {
				Harvest(_currentResourceNode2D, HarvestAmount);
			}
			_currentHarvest = (float)Math.Max(_currentHarvest - delta, 0f);
		}
	}
}
