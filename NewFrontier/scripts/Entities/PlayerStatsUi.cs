using Godot;
using NewFrontier.scripts.Model;

namespace NewFrontier.scripts.Entities;

public partial class PlayerStatsUi : Control {

	private Label _controlPointLabel;
	private Label _oreLabel;
	private Label _gasLabel;
	private Label _crewLabel;
	
	public override void _Ready() {
		_controlPointLabel = GetNode<Label>("HBoxContainer/ControlPointPanel/HBoxContainer2/Panel2/ControlPointLabel");
		_oreLabel = GetNode<Label>("HBoxContainer/OrePanel/HBoxContainer2/Panel2/OreLabel");
		_gasLabel = GetNode<Label>("HBoxContainer/GasPanel/HBoxContainer2/Panel2/GasLabel");
		_crewLabel = GetNode<Label>("HBoxContainer/CrewPanel/HBoxContainer2/Panel2/CrewLabel");
	}

	public void UpdateLabels(PlayerStats stats) {
		_controlPointLabel.Text = $"{stats.CurrentSupply}/{stats.MaxSupply}";
		_oreLabel.Text = $"{stats.CurrentOre}/{stats.MaxOre}";
		_gasLabel.Text = $"{stats.CurrentGas}/{stats.MaxGas}";
		_crewLabel.Text = $"{stats.CurrentCrew}/{stats.MaxCrew}";
	}
}
