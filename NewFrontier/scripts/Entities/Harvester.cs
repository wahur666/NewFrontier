using System;
using System.Linq;
using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Entities;

public partial class Harvester : UnitNode2D {
	private int _currentGasCargo;
	private float _currentHarvest;
	private int _currentOreCargo;

	private ResourceNode2D _currentResourceNode2D;

	private bool _harvesting;
	private PlayerController _playerController;
	[Export] public int HarvestAmount = 25;

	[Export] public float HarvestRate = 0.2f;
	[Export] public int MaxCargo = 500;

	[Export]
	public int CurrentCargo {
		get => _currentGasCargo + _currentOreCargo;
		private set => throw new NotImplementedException();
	}

	public override void _Ready() {
		base._Ready();
		_currentHarvest = HarvestRate;
		_playerController = GetTree().GetFirstNodeInGroup("player") as PlayerController;
		var resources =
			GetTree().GetNodesInGroup("resource").Select(node => node as ResourceNode2D).ToList();
		_currentResourceNode2D = resources[0];
	}

	public override void _Process(double delta) {
		base._Process(delta);
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
		var transferAmount = Math.Min(Math.Min(MaxCargo - CurrentCargo, resourceNode2D.CurrentResource), amount);
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
			var transferAmount = Math.Min(Math.Min(playerController.AvailableOreStorage(), _currentOreCargo),
				harvestAmount);
			_currentOreCargo -= transferAmount;
			playerController.IncreaseOre(transferAmount);
		} else if (_currentGasCargo > 0 && playerController.AvailableGasStorage() > 0) {
			var transferAmount = Math.Min(Math.Min(playerController.AvailableGasStorage(), _currentGasCargo),
				harvestAmount);
			_currentGasCargo -= transferAmount;
			playerController.IncreaseGas(transferAmount);
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
