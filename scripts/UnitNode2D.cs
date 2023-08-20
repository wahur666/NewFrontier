using Godot;
using System;

namespace NewFrontier.scripts;

public partial class UnitNode2D : CharacterBody2D {

	private bool _selected;
	
	[Export]
	public virtual bool Selected {
		get { return _selected; }
		set {
			_selected = value;
			if (_selectionRect is not null) {
				_selectionRect.Selected = value;
			}
		}
	}

	private SelectionRect _selectionRect;

	public override void _Ready() {
		_selectionRect = GetNode<SelectionRect>("SelectionRect");
	}
}
