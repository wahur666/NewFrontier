using System;
using System.Linq;
using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Entities;

public partial class PlanetUi : Control {
	private Label _planetName;
	private Label _planetType;

	private TextureRect _planetTexture;

	private Label _oreLabel;
	private Label _oreMiningLabel;
	private Label _gasLabel;
	private Label _gasMiningLabel;
	private Label _crewLabel;
	private Label _crewMiningLabel;

	private PanelContainer _oreContainer;
	private HBoxContainer _oreProgressContainer;


	private PanelContainer _gasContainer;
	private HBoxContainer _gasProgressContainer;

	private PanelContainer _crewContainer;
	private HBoxContainer _crewProgressContainer;

	public override void _Ready() {
		_planetName = GetNode<Label>("PanelContainer/VBoxContainer/PanelContainer/VBoxContainer/PlanetName");
		_planetType = GetNode<Label>("PanelContainer/VBoxContainer/PanelContainer/VBoxContainer/PlanetType");

		_planetTexture =
			GetNode<TextureRect>(
				"PanelContainer/VBoxContainer/HBoxContainer/PanelContainer/PanelContainer2/PanelContainer/PlanetTexture");
		_oreLabel = GetNode<Label>(
			"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/OreContainer/HBoxContainer/PanelContainer2/VBoxContainer/HBoxContainer/OreLabel");
		_oreMiningLabel =
			GetNode<Label>(
				"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/OreContainer/HBoxContainer/PanelContainer2/VBoxContainer/HBoxContainer/OreMinigLabel");
		_gasLabel = GetNode<Label>(
			"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/GasContainer/HBoxContainer/PanelContainer2/VBoxContainer/HBoxContainer/GasLabel");
		_gasMiningLabel =
			GetNode<Label>(
				"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/GasContainer/HBoxContainer/PanelContainer2/VBoxContainer/HBoxContainer/GasMiningLabel");
		_crewLabel =
			GetNode<Label>(
				"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/CrewContainer/HBoxContainer/PanelContainer2/VBoxContainer/HBoxContainer/CrewLabel");
		_crewMiningLabel =
			GetNode<Label>(
				"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/CrewContainer/HBoxContainer/PanelContainer2/VBoxContainer/HBoxContainer/CrewMiningLabel");

		_oreContainer =
			GetNode<PanelContainer>("PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/OreContainer");
		_gasContainer =
			GetNode<PanelContainer>(
				"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/GasContainer");
		_crewContainer =
			GetNode<PanelContainer>(
				"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/CrewContainer");

		_oreProgressContainer =
			GetNode<HBoxContainer>(
				"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/OreContainer/HBoxContainer/PanelContainer2/VBoxContainer/OreProgressContainer");
		_gasProgressContainer =
			GetNode<HBoxContainer>(
				"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/GasContainer/HBoxContainer/PanelContainer2/VBoxContainer/GasProgressContainer");
		_crewProgressContainer =
			GetNode<HBoxContainer>(
				"PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/CrewContainer/HBoxContainer/PanelContainer2/VBoxContainer/CrewProgressContainer");
	}

	public void UpdateLabels(Planet planet) {
		_planetName.Text = planet.PlanetName;
		_planetType.Text = planet.PlanetType switch {
			PlanetType.Earth => "Earth planet",
			PlanetType.Moon => "Moon",
			PlanetType.GasGiant => "Gas Giant",
			PlanetType.Swamp => "Swamp Planet",
		};
		_oreLabel.Text = $"Ore: {planet.PlanetStats.CurrentOre}/{planet.PlanetStats.MaxOre}";
		_gasLabel.Text = $"Gas: {planet.PlanetStats.CurrentGas}/{planet.PlanetStats.MaxGas}";
		_crewLabel.Text = $"Crew: {planet.PlanetStats.CurrentCrew}/{planet.PlanetStats.MaxCrew}";

		var count = planet.Buildings.Count(building => building is Refinery);
		switch (planet.PlanetType) {
			case PlanetType.Earth:
				_oreMiningLabel.Text = $"{count * 150}/min";
				UpdateProgressContainer(_oreContainer, planet.PlanetStats.CurrentOre, planet.PlanetStats.MaxOre);
				_gasMiningLabel.Text = $"{count * 150}/min";
				UpdateProgressContainer(_gasContainer, planet.PlanetStats.CurrentGas, planet.PlanetStats.MaxGas);
				UpdateProgressContainer(_crewContainer, planet.PlanetStats.CurrentCrew, planet.PlanetStats.MaxCrew);
				break;
			case PlanetType.Moon:
				_oreMiningLabel.Text = $"{count * 150}/min";
				_gasContainer.Modulate = Color.Color8(0, 0, 0, 0);
				_crewContainer.Modulate = Color.Color8(0, 0, 0, 0);
				break;
			case PlanetType.GasGiant:
				_oreContainer.Modulate = Color.Color8(0, 0, 0, 0);
				_gasMiningLabel.Text = $"{count * 150}/min";
				_crewContainer.Modulate = Color.Color8(0, 0, 0, 0);
				break;
			case PlanetType.Swamp:
				_oreContainer.Modulate = Color.Color8(0, 0, 0, 0);
				_gasContainer.Modulate = Color.Color8(0, 0, 0, 0);
				break;
		}
		
	}

	private static void UpdateProgressContainer(PanelContainer panelContainer, int currentValue, int maxValue) {
		var length = panelContainer.GetChildren().Count;
		var availableResourceSteps = (int)(length * currentValue / (double)maxValue);
		var children = panelContainer.GetChildren().ToList().Select(rect => rect as ColorRect).ToList();
		for (int i = 0; i < length; i++){
			var alpha = (byte)(i > availableResourceSteps ? 0 : 255); 
			children[i].Modulate = Color.Color8(0, 0, 0, alpha);
		}
	}
}
