using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using NewFrontier.scripts.Controllers;
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

	private Vector2 TargetDestination;

	protected SelectionRect SelectionRect;

	public Queue<Vector2> Path = new();
	public PlayerController PlayerController;
	public UiController UiController;
	private Node2D canvas;
	private bool _moving;

	public override void _Ready() {
		SelectionRect = GetNode<SelectionRect>("SelectionRect");
		TargetDestination = Position;
		canvas = UiController.Canvas;
		canvas.Draw += CanvasOnDraw;
	}

	public void Init(Vector2 pos, PlayerController playerController, UiController uiController) {
		Position = MapHelpers.GridCoordToGridCenterPos(pos);
		PlayerController = playerController;
		UiController = uiController;
	}

	private void CanvasOnDraw() {
		var points = new List<Vector2> { Position, TargetDestination };
		points.AddRange(Path);
		DrawPath(points);
	}

	public bool InsideSelectionRect(Vector2 position) {
		var size = SelectionRect.Size;
		return AreaHelper.InRect(position, Position + size * new Vector2(.5f, .5f),
			Position - size * new Vector2(.5f, .5f));
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);
		if (Position.DistanceTo(TargetDestination) < 5) {
			if (Path.Count > 0) {
				_moving = true;
				TargetDestination = Path.Dequeue();
			} else {
				_moving = false;
				Velocity = Vector2.Zero;
			}

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

	public void DrawPath(List<Vector2> path) {
		if (!_moving) {
			return;
		}

		for (var i = 0; i < path.Count - 1; i += 1) {
			canvas.DrawLine(path[i], path[i + 1], Colors.Aqua);
		}

		if (path.Count <= 1) {
			return;
		}

		const int squareSize = MapHelpers.Size / 4;
		const int halfSquareSize = squareSize / 2;
		canvas.DrawRect(
			new Rect2(path[^1] - new Vector2(halfSquareSize, halfSquareSize),
				new Vector2(squareSize, squareSize)),
			Colors.Aqua);
		canvas.QueueRedraw();
	}

	public void SetNavigation(List<Vector2> vector2s) {
		TargetDestination = Position;
		if (vector2s.Count > 1
		    // && Position.DistanceTo(vector2s[1]) < vector2s[0].DistanceTo(vector2s[1])
		   ) {
			vector2s.RemoveAt(0);
		}

		this.Path = new Queue<Vector2>(vector2s);
	}
}
