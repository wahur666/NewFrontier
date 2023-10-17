using Godot;
using System;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Entities;

public partial class UnitNode2D : CharacterBody2D {

	private bool _selected;
	private int _speed = 300;
	private int _acceleration = 7;
	
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
	public Vector2 TargetDestination;

	protected SelectionRect SelectionRect;

	public override void _Ready() {
		SelectionRect = GetNode<SelectionRect>("SelectionRect");
		TargetDestination = Position;
	}

	public bool InsideSelectionRect(Vector2 position) {
		var size = SelectionRect.Size;
		return AreaHelper.InRect(position, Position + size * new Vector2(.5f, .5f), Position - size * new Vector2(.5f, .5f));
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);
		if (Position.DistanceTo(TargetDestination) < 5) {
			return;
		}

		var direction = TargetDestination - GlobalPosition;
		direction = direction.Normalized();
		Velocity = Velocity.Lerp(direction * _speed, (float)(_acceleration * delta));
		MoveAndSlide();
	}
}
