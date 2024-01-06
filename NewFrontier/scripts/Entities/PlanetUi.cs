using Godot;

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
	private PanelContainer _oreProgressContainer;
	
	
	private PanelContainer _gasContainer;
	private PanelContainer _gasProgressContainer;
	
	private PanelContainer _crewContainer;
	private PanelContainer _crewProgressContainer;
	
	public override void _Ready() {
		
	}

}
