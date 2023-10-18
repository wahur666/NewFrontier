using Godot;
using System;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Entities;

public partial class UnitNode2D : CharacterBody2D {

	private bool _selected;
	private int _speed = 300;
	
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
    
		// Calculate the angle between the current rotation and the direction vector
		float targetRotation = Mathf.Atan2(direction.Y, direction.X);
    
		// Ensure the angle is between -PI and PI
		while (targetRotation - Rotation > Mathf.Pi) {
			targetRotation -= 2 * Mathf.Pi;
		}
		while (targetRotation - Rotation < -Mathf.Pi) {
			targetRotation += 2 * Mathf.Pi;
		}
    
		// Define a constant rotation speed
		float rotationSpeed = 2.0f; // You can adjust this value as needed
    
		// Calculate the rotation difference
		float rotationDifference = targetRotation - Rotation + Mathf.Pi / 2;
    
		// Determine the shortest path to the target rotation
		if (rotationDifference > Mathf.Pi) {
			rotationDifference -= 2 * Mathf.Pi;
		} else if (rotationDifference < -Mathf.Pi) {
			rotationDifference += 2 * Mathf.Pi;
		}
    
		// Rotate at a constant speed
		if (Mathf.Abs(rotationDifference) > 0.03) {
			float rotationDirection = Mathf.Sign(rotationDifference);
			Rotation += rotationDirection * rotationSpeed * (float)delta;
		} else {
			// When the rotation aligns with the direction vector, assign the velocity
			Velocity = direction * _speed * (float)delta * 20;
			MoveAndSlide();
		}
	}


}
