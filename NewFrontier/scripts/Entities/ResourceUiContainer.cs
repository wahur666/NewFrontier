using System.Linq;
using Godot;

namespace NewFrontier.scripts.Entities;

public partial class ResourceUiContainer : PanelContainer {

	[Export] public Texture2D Icon;
	[Export] public string ResourceText;

	private TextureRect _iconTexture;
	private Label _resourceLabel;
	private Label _resourceMiningLabel;
	private BoxContainer _resourceProgressContainer;
	
	public override void _Ready() {
		_iconTexture = GetNode<TextureRect>("HBoxContainer/PanelContainer/IconTexture");
		_iconTexture.Texture = Icon;
		_resourceLabel = GetNode<Label>("HBoxContainer/PanelContainer2/VBoxContainer/HBoxContainer/ResourceLabel");
		_resourceMiningLabel =
			GetNode<Label>("HBoxContainer/PanelContainer2/VBoxContainer/HBoxContainer/ResourceMinigLabel");
		_resourceProgressContainer =
			GetNode<BoxContainer>(
				"HBoxContainer/PanelContainer2/VBoxContainer/PanelContainer/ResourceProgressContainer");
	}

	public void UpdateResourceLabel(int currentValue, int maxValue) {
		_resourceLabel.Text = $"{ResourceText}: {currentValue}/{maxValue}";
	}

	public void UpdateResourceMiningLabel(int amount) {
		_resourceMiningLabel.Text = $"{amount}/min";
	}

	public void UpdateProgressContainer(int currentValue, int maxValue) {
		var length = _resourceProgressContainer.GetChildren().Count;
		var availableResourceSteps = (int)(length * currentValue / (double)maxValue);
		var children = _resourceProgressContainer.GetChildren().ToList().Select(rect => rect as ColorRect).ToList();
		for (int i = 0; i < length; i++){
			var alpha = (byte)(i >= availableResourceSteps ? 0 : 255); 
			children[i].Modulate = Color.Color8(255, 255, 255, alpha);
		}
	}
	
}
