using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts;

public partial class Harvester : Node2D {
	[Export] public int MaxCargo = 500;
	private int _currentGasCargo = 0;
	private int _currentOreCargo = 0;

	[Export]
	public int CurrentCargo {
		get => _currentGasCargo + _currentOreCargo;
		private set { throw new NotImplementedException(); }
	}

	[Export] public float HarvestRate = 0.2f;
	private float _currentHarvest;
	[Export] public int HarvestAmount = 25;

	private bool _harvesting = false;

	private ResourceNode2D _currentResourceNode2D = null;
	private PlayerController _playerController = null;

	public override void _Ready() {
		_currentHarvest = HarvestRate;
		_playerController = GetTree().GetFirstNodeInGroup("player") as PlayerController;
		GD.Print("Player Controller" + _playerController);
		List<ResourceNode2D> resources =
			GetTree().GetNodesInGroup("resource").Select(node => node as ResourceNode2D).ToList();
		_currentResourceNode2D = resources[0];
		GD.Print(_currentResourceNode2D);
		GD.Print(resources, resources.Count);
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

		if (Input.IsActionJustPressed("unload") && _playerController is not null) {
			GD.Print("unloading");
			Unload(_playerController, HarvestAmount);
		}
	}

	public void Harvest(ResourceNode2D resourceNode2D, int amount) {
		var transferAmount = Math.Min(Math.Min((MaxCargo - CurrentCargo), resourceNode2D.CurrentResource), amount);
		resourceNode2D.CurrentResource -= transferAmount;


		if (resourceNode2D.Resource == ResourceType.Gas) {
			_currentGasCargo += transferAmount;
		} else {
			_currentOreCargo += transferAmount;
		}

		resourceNode2D.UpdateScale();
	}

	private void Unload(PlayerController playerController, int harvestAmount) {
		if (_currentOreCargo > 0 && playerController.AvailableOreStorage() > 0) {
			var transferAmount = Math.Min(Math.Min(playerController.AvailableOreStorage(), _currentOreCargo), harvestAmount);
			_currentOreCargo -= transferAmount;
			playerController.CurrentOre += transferAmount;
		}

		if (_currentGasCargo > 0 && playerController.AvailableGasStorage() > 0) {
			var transferAmount = Math.Min(Math.Min(playerController.AvailableGasStorage(), _currentGasCargo), harvestAmount);
			_currentGasCargo -= transferAmount;
			playerController.CurrentGas += transferAmount;
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
