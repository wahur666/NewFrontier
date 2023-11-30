using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Entities;

public partial class UnitNode2D : CharacterBody2D {
	private bool _moving;
	private bool _selected;
	private int _speed = 300;
	private TravelState _travelState = TravelState.NotTraveling;
	private Node2D _canvas;

	private double _travelTime = 1d;
	private double _currentTravelTime = 0d;
	private float _jumpDistance;

	private Queue<GameNode> _navPoints = new();
	private PlayerController _playerController;

	private SelectionRect _selectionRect;

	private Vector2 _targetDestination;
	private UiController _uiController;
	private float _rotationSpeed = 2.0f;
	private bool _rotatedCorrectly;

	[Export]
	public bool Selected {
		get => _selected;
		set {
			_selected = value;
			if (_selectionRect is not null) {
				_selectionRect.Selected = value;
			}
		}
	}

	public override void _Ready() {
		_selectionRect = GetNode<SelectionRect>("SelectionRect");
		_targetDestination = Position;
		_canvas = _uiController.Canvas;
		_canvas.Draw += CanvasOnDraw;
	}

	public void Init(Vector2 pos, PlayerController playerController, UiController uiController) {
		Position = MapHelpers.GridCoordToGridCenterPos(pos);
		_playerController = playerController;
		_uiController = uiController;
	}

	private void CanvasOnDraw() {
		if (!Selected) {
			return;
		}

		var points = new List<Vector2> { Position, _targetDestination };
		points.AddRange(_navPoints.Select(x => MapHelpers.GridCoordToGridCenterPos(x.Position)));
		DrawPath(points);
	}

	public bool InsideSelectionRect(Vector2 position) {
		var size = _selectionRect.Size;
		return AreaHelper.InRect(position, Position + (size * new Vector2(.5f, .5f)),
			Position - (size * new Vector2(.5f, .5f)));
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);

		switch (_travelState) {
			case TravelState.PrepareForTraveling:
				UpdatePrepareForTraveling(delta);
				break;
			case TravelState.Traveling:
				UpdateTraveling(delta);
				break;
			case TravelState.EndTraveling:
				UpdateEndTraveling(delta);
				break;
			case TravelState.NotTraveling:
			default:
				UpdateNotTraveling(delta);
				break;
		}
	}

	private void DrawPath(List<Vector2> path) {
		if (!_moving) {
			return;
		}

		for (var i = 0; i < path.Count - 1; i += 1) {
			_canvas.DrawLine(path[i], path[i + 1], Colors.Aqua);
		}

		if (path.Count <= 1) {
			return;
		}

		const int squareSize = MapHelpers.DrawSize / 4;
		const int halfSquareSize = squareSize / 2;
		_canvas.DrawRect(
			new Rect2(path[^1] - new Vector2(halfSquareSize, halfSquareSize),
				new Vector2(squareSize, squareSize)),
			Colors.Aqua);
		_canvas.QueueRedraw();
	}

	public void SetNavigation(List<GameNode> vector2S) {
		if (_travelState is TravelState.Traveling or TravelState.EndTraveling) {
			return;
		}

		_targetDestination = Position;
		if (vector2S.Count > 1) {
			vector2S.RemoveAt(0);
		}

		_navPoints = new Queue<GameNode>(vector2S);
	}

	private void StopBody() {
		_moving = false;
		Velocity = Vector2.Zero;
	}

	private void UpdateNotTraveling(double delta) {
		if (Position.DistanceTo(_targetDestination) < 5) {
			if (_navPoints.Count > 0) {
				_moving = true;
				var target = _navPoints.Dequeue();
				_targetDestination = MapHelpers.GridCoordToGridCenterPos(target.Position);
				if (!target.HasWormhole || _navPoints.Count <= 0 || !_navPoints.Peek().HasWormhole ||
				    _navPoints.Peek().Index == target.Index) {
					return;
				}

				this._travelState = TravelState.PrepareForTraveling;
				GD.Print("prepare for travelling");
				_rotatedCorrectly = false;
			} else {
				StopBody();
			}

			return;
		}

		MoveToTarget(delta);
	}

	public Vector2 GridPosition() {
		return MapHelpers.PosToGrid(GlobalPosition);
	}

	private void MoveToTarget(double delta, float speed = 1) {
		var rotationDifference = CalculateRotationDifference();
		if (Mathf.Abs(rotationDifference) > 0.03) {
			RotateTowardsTarget(delta, rotationDifference, _rotationSpeed);
			return;
		}

		MoveTowardsTarget(delta, speed);
	}

	private void RotateTowardsTarget(double delta, float rotationDifference, float rotationSpeed) {
		var rotationDirection = Mathf.Sign(rotationDifference);
		Rotation += rotationDirection * rotationSpeed * (float)delta;
	}

	private void MoveTowardsTarget(double delta, float speed = 1) {
		// When the rotation aligns with the direction vector, assign the velocity
		var direction = GlobalPosition.DirectionTo(_targetDestination);
		Velocity = direction * _speed * (float)delta * 20 * speed;
		MoveAndSlide();
	}

	private float CalculateRotationDifference() {
		var targetRotation = CalculateTargetRotation();
		var rotationDifference = targetRotation - Rotation + (Mathf.Pi / 2);

		// Determine the shortest path to the target rotation
		if (rotationDifference > Mathf.Pi) {
			rotationDifference -= Mathf.Tau;
		} else if (rotationDifference < -Mathf.Pi) {
			rotationDifference += Mathf.Tau;
		}

		return rotationDifference;
	}

	private float CalculateTargetRotation() {
		var targetRotation = GlobalPosition.AngleToPoint(_targetDestination);
		while (targetRotation - Rotation > Mathf.Pi) {
			targetRotation -= Mathf.Tau;
		}

		while (targetRotation - Rotation < -Mathf.Pi) {
			targetRotation += Mathf.Tau;
		}

		return targetRotation;
	}

	private void UpdatePrepareForTraveling(double delta) {
		// Wait until everybody is read for jump
		StopBody();
		if (!_rotatedCorrectly) {
			var rotationDifference = CalculateRotationDifference();
			if (Mathf.Abs(rotationDifference) > 0.03) {
				RotateTowardsTarget(delta, rotationDifference, _rotationSpeed);
				return;
			}
		}

		_rotatedCorrectly = true;
		if (_currentTravelTime < _travelTime) {
			_jumpDistance = GlobalPosition.DistanceTo(_targetDestination);
			_currentTravelTime += delta;
			return;
		}

		if (GlobalPosition.DistanceTo(_targetDestination) < 5) {
			var target = _navPoints.Dequeue();
			_targetDestination = MapHelpers.GridCoordToGridCenterPos(target.Position);
			_currentTravelTime = 0;
			_travelState = TravelState.Traveling;
			GD.Print("Travelling");
		} else {
			MoveToTarget(delta, 4);
			var scale = Mathf.Clamp(GlobalPosition.DistanceTo(_targetDestination) / _jumpDistance, 0.1f, 1);
			Scale = new Vector2(scale, scale);
		}
	}

	private void UpdateTraveling(double delta) {
		Visible = false;
		if (_currentTravelTime < _travelTime) {
			_currentTravelTime += delta;
			return;
		}

		GlobalPosition = _targetDestination;
		_currentTravelTime = 0;
		var target = _navPoints.Dequeue();
		_targetDestination = MapHelpers.GridCoordToGridCenterPos(target.Position);
		Rotation = GlobalPosition.AngleToPoint(_targetDestination) + Mathf.Pi / 2;
		_travelState = TravelState.EndTraveling;
		GD.Print("End traveling");
	}

	private void UpdateEndTraveling(double delta) {
		Visible = true;
		// speed up ship until its reached the next position
		if (GlobalPosition.DistanceTo(_targetDestination) < 5) {
			if (_navPoints.Count > 0) {
				var target = _navPoints.Dequeue();
				_targetDestination = MapHelpers.GridCoordToGridCenterPos(target.Position);
			}
		} else {
			var scale = Mathf.Clamp(1 - GlobalPosition.DistanceTo(_targetDestination) / _jumpDistance, 0.1f, 1);
			Scale = new Vector2(scale, scale);
			MoveToTarget(delta, 4);
			return;
		}

		if (_navPoints.Count == 0) {
			StopBody();
		}

		Scale = Vector2.One;
		_travelState = TravelState.NotTraveling;
	}
}
