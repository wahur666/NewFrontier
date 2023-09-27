using Godot;
using System;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Entities;

public partial class UnitNode2D : CharacterBody2D {

	private bool _selected;
	
	[Export]
	public virtual bool Selected {
		get { return _selected; }
		set {
			_selected = value;
			if (SelectionRect is not null) {
				SelectionRect.Selected = value;
			}
		}
	}

	protected SelectionRect SelectionRect;

	public override void _Ready() {
		SelectionRect = GetNode<SelectionRect>("SelectionRect");
	}

	public bool InsideSelectionRect(Vector2 position) {
		var size = SelectionRect.Size;
		return AreaHelper.InRect(position, Position + size * new Vector2(.5f, .5f), Position - size * new Vector2(.5f, .5f));
	}
}
