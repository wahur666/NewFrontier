using System;
using System.Collections.Generic;
using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Interfaces;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Entities;

public partial class BuildingNode2D : Node2D, IBase, ISelectable, IBuildable {
	private const int PlanetImgSize = 34;

	protected PlayerController PlayerController;
	private bool _mouseOver;

	public bool BuildingShade = true;
	public List<object> BuildQueue;
	public List<object> Items;
	public List<string> PreRequisites;

	public Node2D Instance { get => this; }
	public int[] Place { get; set; } = Array.Empty<int>();
	public Planet Planet { get; set; }
	public string BuildingName { get; set; }
	public Sprite2D BuildingSprite { get; set; }
	
	[Export] public int Wide { get; set; } = 1;
	[Export] public SnapOption SnapOption { get; set; } = SnapOption.Planet;

	[Export] public Texture2D Icon { get; set; }
	private bool _selected;

	public bool IsUnitSelectable => false;
	public Vector2 Pos { get => Position; } 

	[Export]
	public bool Selected {
		get => _selected;
		set {
			_selected = value;
			if (SelectionRect is not null) {
				AsISelectable.SetSelection(value);
			}
		}
	}

	[ExportGroup("Stats")] [Export] public int MaxHealth { get; set; }
	[ExportGroup("Stats")] [Export] public int CurrentHealth { get; set; }

	private ISelectable AsISelectable => this;
	public SelectionRect SelectionRect { get; private set; }


	public void Destroy() {
		// Play Anim
		QueueFree();
	}

	public override void _Ready() {
		if (SnapOption == SnapOption.Planet) {
			Scale = new Vector2(Planet.Radius / (float)PlanetImgSize, Planet.Radius / (float)PlanetImgSize);
		}

		BuildingSprite = GetNode<Sprite2D>("Sprite2D");
		SelectionRect = GetNode<SelectionRect>("SelectionRect");
		SelectionRect.MouseEntered += () => _mouseOver = true;
		SelectionRect.MouseExited += () => _mouseOver = false;
	}

	public void Init(PlayerController playerController, string name, int zIndex = 10) {
		PlayerController = playerController;
		BuildingName = name;
		ZIndex = zIndex;
	}


	public bool InsideSelectionRect(Vector2 position) {
		return _mouseOver;
	}
}
