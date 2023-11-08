using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.Entities;

namespace NewFrontier.scripts.UI;

public partial class LeftControls : Control {
	private Control _buttonContainer;
	private Control _iconContainer;
	private Button _button1;
	private Button _button2;
	private Button _button3;
	public PlayerController PlayerController;
	private PackedScene _fabricatorIcon;
	private PackedScene _harvesterIcon;

	private int _iconSize = 32;
	private int _spacing = 5;

	public bool OverUiElement {
		get;
		private set;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_fabricatorIcon = GD.Load<PackedScene>("res://scenes/fabricator_icon.tscn");
		_harvesterIcon = GD.Load<PackedScene>("res://scenes/harvester_icon.tscn");
		_buttonContainer = GetNode<Control>("Panel/Container");
		_iconContainer = GetNode<Control>("Panel/Cont2");
		_button1 = GetNode<Button>("Panel/Container/Button");
		_button2 = GetNode<Button>("Panel/Container/Button2");
		_button3 = GetNode<Button>("Panel/Container/Button3");
	}

	public void Init(PlayerController playerController) {
		PlayerController = playerController;
		_button1.Pressed += PlayerController.CreateBuilding1;
		_button2.Pressed += PlayerController.CreateBuilding2;
		_button3.Pressed += PlayerController.CreateBuilding3;
		_button1.MouseEntered += MouseEnteredPanelElement;
		_button2.MouseEntered += MouseEnteredPanelElement;
		_button3.MouseEntered += MouseEnteredPanelElement;

		_iconContainer.MouseEntered += MouseEnteredPanelElement;
		_iconContainer.MouseExited += MouseExitedPanelElement;

		_buttonContainer.MouseEntered += MouseEnteredPanelElement;
		_buttonContainer.MouseExited += MouseExitedPanelElement;
		PlayerController.LeftControls = this;
	}

	public void CalculateSelectedUnits(List<UnitNode2D> units) {
		_iconContainer.GetChildren().ToList().ForEach(x => x.QueueFree());
		_iconContainer.GetChildren().Clear();
		GD.Print("Cleaning up");
		const int startY = 10; // Adjust the starting Y-coordinate as needed
		var maxSquares = Math.Min(22, units.Count); // Maximum number of squares to display
		var rowIndex = 0;
		var squaresDrawn = 0;
		while (squaresDrawn < maxSquares && rowIndex < 4) {
			var rowCount = (rowIndex % 2 == 0) ? 6 : 5;
			var startX = (rowIndex % 2 == 0) ? 10 : 10 + (_iconSize / 2) + _spacing; // Indent odd rows
			var endIndex = Math.Min(squaresDrawn + rowCount, maxSquares);
			DrawRow(rowCount, startX, startY + (rowIndex * (_iconSize + _spacing)), squaresDrawn, endIndex, units);
			squaresDrawn += rowCount;
			rowIndex++;
		}
	}

	private void DrawRow(int rowCount, int startX, int startY, int startNumber, int endIndex, List<UnitNode2D> units) {
		for (var i = 0; i < rowCount && startNumber < endIndex; i++) {
			GD.Print("Index: ", startNumber);
			var a = units[startNumber];
			PackedScene scene = a switch {
				Fabricator => _fabricatorIcon,
				Harvester => _harvesterIcon,
				_ => _harvesterIcon
			};;
			DrawIcon(startX + (i * (_iconSize + _spacing)), startY, scene, a);
			startNumber++;
		}
	}
	
	private void DrawIcon(int x, int y, PackedScene scene, UnitNode2D unitNode2D) {
		var icon = scene.Instantiate() as UnitIcon;
		icon.Position = new Vector2(x, y);
		icon.MouseEntered += MouseEnteredPanelElement;
		icon.Unit = unitNode2D;
		icon.PlayerController = PlayerController;
		_iconContainer.AddChild(icon);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	private void MouseEnteredPanelElement() {
		OverUiElement = true;
		PlayerController.SetBuildingShadeVisibility(false);
		PlayerController.OverGui = true;
	}

	private void MouseExitedPanelElement() {
		OverUiElement = false;
		PlayerController.SetBuildingShadeVisibility(true);
		PlayerController.OverGui = false;
	}

	public void SetBuildingContainerVisibility(bool visible) {
		_buttonContainer.Visible = visible;
		_iconContainer.Visible = !visible;
	}
}