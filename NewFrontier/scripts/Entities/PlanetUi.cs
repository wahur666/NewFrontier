using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Entities;

public partial class PlanetUi : Control {
	private Label _planetName;
	private Label _planetType;

	private TextureRect _planetTexture;

	private ResourceUiContainer _oreContainer;
	private ResourceUiContainer _gasContainer;
	private ResourceUiContainer _crewContainer;

	public override void _Ready() {
		_planetName = GetNode<Label>("PanelContainer/VBoxContainer/PanelContainer/VBoxContainer/PlanetName");
		_planetType = GetNode<Label>("PanelContainer/VBoxContainer/PanelContainer/VBoxContainer/PlanetType");

		_planetTexture =
			GetNode<TextureRect>(
				"PanelContainer/VBoxContainer/HBoxContainer/PanelContainer/PanelContainer2/PanelContainer/PlanetTexture");

		_oreContainer =
			GetNode<ResourceUiContainer>("PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/OreContainer");
		_gasContainer =
			GetNode<ResourceUiContainer>("PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/GasContainer");
		_crewContainer =
			GetNode<ResourceUiContainer>("PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/CrewContainer");

	}

	public void UpdateLabels(Planet planet) {
		_planetName.Text = planet.PlanetName;
		_planetType.Text = planet.PlanetType switch {
			PlanetType.Earth => "Earth planet",
			PlanetType.Moon => "Moon",
			PlanetType.GasGiant => "Gas Giant",
			PlanetType.Swamp => "Swamp Planet",
		};
		_oreContainer.UpdateResourceLabel(planet.PlanetStats.CurrentOre, planet.PlanetStats.MaxOre);
		_gasContainer.UpdateResourceLabel(planet.PlanetStats.CurrentGas, planet.PlanetStats.MaxGas);
		_crewContainer.UpdateResourceLabel(planet.PlanetStats.CurrentCrew, planet.PlanetStats.MaxCrew);

		var refineries = planet.Buildings.OfType<Refinery>().ToList();
		switch (planet.PlanetType) {
			case PlanetType.Earth:
				_oreContainer.UpdateResourceMiningLabel(refineries.Sum(refinery => refinery.OreMiningSpeed));
				_oreContainer.UpdateProgressContainer(planet.PlanetStats.CurrentOre, planet.PlanetStats.MaxOre);
				_gasContainer.UpdateResourceMiningLabel(refineries.Sum(refinery => refinery.GasMiningSpeed));
				_gasContainer.UpdateProgressContainer(planet.PlanetStats.CurrentGas, planet.PlanetStats.MaxGas);
				// UpdateProgressContainer(_crewProgressContainer, planet.PlanetStats.CurrentCrew, planet.PlanetStats.MaxCrew);
				break;
			case PlanetType.Moon:
				_oreContainer.UpdateResourceMiningLabel(refineries.Sum(refinery => refinery.OreMiningSpeed));
				_gasContainer.Modulate = Color.Color8(0, 0, 0, 0);
				_crewContainer.Modulate = Color.Color8(0, 0, 0, 0);
				break;
			case PlanetType.GasGiant:
				_oreContainer.Modulate = Color.Color8(0, 0, 0, 0);
				_gasContainer.UpdateResourceMiningLabel(refineries.Sum(refinery => refinery.GasMiningSpeed));
				_crewContainer.Modulate = Color.Color8(0, 0, 0, 0);
				break;
			case PlanetType.Swamp:
				_oreContainer.Modulate = Color.Color8(0, 0, 0, 0);
				_gasContainer.Modulate = Color.Color8(0, 0, 0, 0);
				break;
		}
	}
}
