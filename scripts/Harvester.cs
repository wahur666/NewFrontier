using System;
using System.Text.RegularExpressions;
using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts;

public partial class Harvester : Node2D {
	[Export] public int MaxCargo = 500;
	private int _currentGasCargo = 0;
	private int _currentOreCargo = 0;
	[Export] public int CurrentCargo {
		get => _currentGasCargo + _currentOreCargo;
		private set { throw new NotImplementedException(); }
	}

	[Export] public float HarvestRate = 0.2f;
	private float _currentHarvest;
	[Export] public int HarvestAmount = 25;

	private bool _harvesting = false;

	private ResourceNode2D _currentResourceNode2D = null;

	public override void _Ready() {
		_currentHarvest = HarvestRate;
		_currentResourceNode2D = GetParent().GetNode<ResourceNode2D>("Asteroid");
		GD.Print(_currentResourceNode2D);
	}

	public void Harvest(ResourceNode2D resourceNode2D, int amount) {
		var transferAmount =
			Math.Min(Math.Min((MaxCargo - (_currentGasCargo + _currentOreCargo)), resourceNode2D.CurrentResource),
				amount);
		resourceNode2D.CurrentResource -= transferAmount;

		
		if (resourceNode2D.Resource == ResourceType.Gas) {
			_currentGasCargo += transferAmount;
		} else {
			_currentOreCargo += transferAmount;
		}

		resourceNode2D.UpdateScale();
	}

	public override void _Process(double delta) {
		if (_currentResourceNode2D is not null && _harvesting) {
			if (_currentHarvest == 0) {
				Harvest(_currentResourceNode2D, HarvestAmount);
				CheckResourceToFree();
			}

			_currentHarvest = (float)Math.Max(_currentHarvest - delta, 0f);
		}

		if (Input.IsActionJustPressed("harvest") && _currentResourceNode2D is not null) {
			GD.Print("harvesting");
			Harvest(_currentResourceNode2D, HarvestAmount);
			CheckResourceToFree();
		}
	}

	private void CheckResourceToFree() {
		if (_currentResourceNode2D.CurrentResource == 0) {
			_currentResourceNode2D.QueueFree();
			_currentResourceNode2D = null;
			GD.Print("Freeing resource");
		}
	}
}
