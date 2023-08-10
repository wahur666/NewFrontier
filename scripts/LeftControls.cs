using Godot;
using System;

namespace NewFrontier.scripts;

public partial class LeftControls : Control {
	private Panel _buttonPanel;
	private Button _button1;
	private Button _button2;
	private Button _button3;
	private PlayerController _playerController;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_playerController = GetTree().GetFirstNodeInGroup("player") as PlayerController;
		_buttonPanel = GetNode<Panel>("Panel");
		_button1 = GetNode<Button>("Panel/Container/Button");
		_button2 = GetNode<Button>("Panel/Container/Button2");
		_button3 = GetNode<Button>("Panel/Container/Button3");

		_button1.Pressed += _playerController.CreateBuilding1;
		_button2.Pressed += _playerController.CreateBuilding2;
		_button3.Pressed += _playerController.CreateBuilding3;
		
		_buttonPanel.MouseEntered += MouseEnteredPanelElement;
		_buttonPanel.MouseExited += MouseExitedPanelElement;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	private void MouseEnteredPanelElement() {
		_playerController.SetBuildingShadeVisibility(false);
	}

	private void MouseExitedPanelElement() {
		_playerController.SetBuildingShadeVisibility(true);
	}

}
