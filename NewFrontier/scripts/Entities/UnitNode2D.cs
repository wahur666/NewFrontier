using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Interfaces;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Entities;

public partial class UnitNode2D : CharacterBody2D, IBase, ISelectable {
	private Node2D _canvas;
	private double _currentTravelTime;
	private float _jumpDistance;
	private bool _moving;

	private Queue<GameNode> _navPoints = new();
	protected PlayerController PlayerController;
	private bool _rotatedCorrectly;
	private float _rotationSpeed = 2.0f;
	private bool _selected;

	private int _speed = 300;

	private Vector2 _targetDestination;
	private TravelState _travelState = TravelState.NotTraveling;

	private double _travelTime = 1d;
	private UiController _uiController;
	private ProgressBar healthbar;

	private List<GameNode> currentNodes;

	/// <summary>
	///     Big ship (2x2 tiles) or little ship (1x1 tile)
	///     https://youtu.be/1AaNj7W4AKo
	/// </summary>
	[Export] public bool BigShip;

	private int _currentHealth;
	private MapGrid _mapGrid;
	private Queue<Vector2> _smoothedPoints = new();
	private Vector2 _targetDestinationPoint;

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
	
	
	[Export] public Texture2D Icon { get; set; }
	public virtual bool IsUnitSelectable => true;

	private ISelectable AsISelectable => this;
	public Vector2 Pos { get => Position; } 

	[ExportGroup("Stats")] [Export] public int MaxHealth { get; set; }

	[ExportGroup("Stats")]
	[Export]
	public int CurrentHealth {
		get => _currentHealth;
		set {
			_currentHealth = value;
			if (healthbar is not null) {
				healthbar.Value = CurrentHealth / (float)MaxHealth * 100;
			}
		}
	}

	public Node2D Instance { get => this; }

	public void Destroy() {
		PlayerController.RemoveUnit(this);
		QueueFree();
	}

	public override void _Ready() {
		SelectionRect = GetNode<SelectionRect>("SelectionRect");
		healthbar = GetNode<ProgressBar>("Node2D/Healthbar");
		_targetDestination = Position;
		_targetDestinationPoint = Position;
	}

	public void Init(Vector2 pos, PlayerController playerController, UiController uiController, MapGrid mapGrid) {
		Position = GetTargetPosition(pos);
		PlayerController = playerController;
		_uiController = uiController;
		_canvas = _uiController.Canvas;
		_canvas.Draw += CanvasOnDraw;
		_mapGrid = mapGrid;
		
		if (BigShip) {
			currentNodes = [];
		} else {
			currentNodes = [_mapGrid.GetGameNode(MapHelpers.PosToGrid(GlobalPosition))];
			currentNodes.ForEach(x => x.Occupied = true);
		}
	}

	/// <summary>
	/// Returns the correct Grid Position based on the unit size
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	private Vector2 GetTargetPosition(Vector2 position) {
		return BigShip ? MapHelpers.GridCoordToGridPointPos(position) : MapHelpers.GridCoordToGridCenterPos(position);
	}

	private void CanvasOnDraw() {
		if (!Selected) {
			return;
		}
		
		var points = new List<Vector2> { Position, _targetDestination };
		points.AddRange(_navPoints.Select(x => GetTargetPosition(x.Position)));
		var z = PathHelper.CalculateSmoothPath(points);
		DrawPath(points, Colors.Aqua);
		DrawPath(z, Colors.Red);
	}

	public bool InsideSelectionRect(Vector2 position) {
		var size = SelectionRect.Size;
		return AreaHelper.InRect(position, Position + (size * new Vector2(.5f, .5f)),
			Position - (size * new Vector2(.5f, .5f)));
	}

	public override void _Process(double delta) {
		base._Process(delta);
		if (healthbar is not null) {
			healthbar.GlobalPosition = GlobalPosition + new Vector2(-50, -100);
		}

		if (BigShip) {
			
		} else {
			currentNodes.ForEach(x => x.Occupied = false);
			currentNodes = [_mapGrid.GetGameNode(MapHelpers.PosToGrid(GlobalPosition))];
			currentNodes.ForEach(x => x.Occupied = true);
		}
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

	public SelectionRect SelectionRect { get; private set; }

	private void DrawPath(List<Vector2> path, Color color) {
		if (!_moving) {
			return;
		}

		for (var i = 0; i < path.Count - 1; i += 1) {
			_canvas.DrawLine(path[i], path[i + 1], color);
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

		if (_navPoints.Count > 0) {
			_navPoints.ToList()[^1].PreOccupied = false;
		}

		_targetDestination = Position;
		_targetDestinationPoint = Position;
		if (vector2S.Count > 1) {
			vector2S.RemoveAt(0);
		}
		
		_navPoints = new Queue<GameNode>(vector2S);
		_smoothedPoints = new Queue<Vector2>(GenerateSmoothNavPoints(_navPoints.ToList()));
	}

	private List<Vector2> GenerateSmoothNavPoints(List<GameNode> nodes) {
		return PathHelper.CalculateSmoothPath(nodes.Select(x => GetTargetPosition(x.Position)).ToList());
	}

	private void StopBody() {
		_moving = false;
		Velocity = Vector2.Zero;
	}

	private void UpdateNotTraveling(double delta) {
		if (Position.DistanceTo(_targetDestinationPoint) < 5) {
			if (GridPosition(_targetDestination) == GridPosition(Position)) {
				if (_navPoints.Count > 0) {
					_moving = true;
					var target = _navPoints.Dequeue();
					_targetDestination = GetTargetPosition(target.Position);
					if (!target.HasWormhole || _navPoints.Count <= 0 || !_navPoints.Peek().HasWormhole ||
					    _navPoints.Peek().SectorIndex == target.SectorIndex) {
						return;
					}

					_travelState = TravelState.PrepareForTraveling;
					GD.Print("prepare for travelling");
					_rotatedCorrectly = false;
				} else {
					if (_smoothedPoints.Count > 0) {
						_smoothedPoints = new([_targetDestination]);
					}
					StopBody();
				}
			}

			if (_smoothedPoints.Count > 0) {
				_targetDestinationPoint = _smoothedPoints.Dequeue();
			} else {
				StopBody();
			}
			
			return;
		}

		MoveToTarget(delta);
	}

	public Vector2 GridPosition(Vector2 position) {
		return BigShip ? MapHelpers.PosToGridPoint(position) : MapHelpers.PosToGrid(position);
	}

	private void MoveToTarget(double delta, float speed = 1) {
		var rotationDifference = CalculateRotationDifference();
		if (Mathf.Abs(rotationDifference) > 0.03) {
			RotateTowardsTarget(delta, rotationDifference, _rotationSpeed);
			if (Mathf.Abs(rotationDifference) < Mathf.Pi / 2f) {
				MoveTowardsTarget(delta, speed * (1 - Mathf.Abs(rotationDifference) / (Mathf.Pi / 2)));
			}
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
		var direction = GlobalPosition.DirectionTo(_targetDestinationPoint);
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
		var targetRotation = GlobalPosition.AngleToPoint(_targetDestinationPoint);
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
		_smoothedPoints = new Queue<Vector2>(_navPoints.Select(x => GetTargetPosition(x.Position)).ToList());
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
			_jumpDistance = GlobalPosition.DistanceTo(_targetDestinationPoint);
			_currentTravelTime += delta;
			return;
		}

		if (GlobalPosition.DistanceTo(_targetDestinationPoint) < 5) {
			var target = _navPoints.Dequeue();
			_targetDestinationPoint = GetTargetPosition(target.Position);
			_currentTravelTime = 0;
			_travelState = TravelState.Traveling;
			GD.Print("Travelling");
		} else {
			MoveToTarget(delta, 4);
			var scale = Mathf.Clamp(GlobalPosition.DistanceTo(_targetDestinationPoint) / _jumpDistance, 0.1f, 1);
			Scale = new Vector2(scale, scale);
		}
	}

	private void UpdateTraveling(double delta) {
		Visible = false;
		if (_currentTravelTime < _travelTime) {
			_currentTravelTime += delta;
			return;
		}

		GlobalPosition = _targetDestinationPoint;
		_currentTravelTime = 0;
		var target = _navPoints.Dequeue();
		_targetDestinationPoint = GetTargetPosition(target.Position);
		Rotation = GlobalPosition.AngleToPoint(_targetDestinationPoint) + (Mathf.Pi / 2);
		_travelState = TravelState.EndTraveling;
		GD.Print("End traveling");
	}

	private void UpdateEndTraveling(double delta) {
		Visible = true;
		// speed up ship until its reached the next position
		if (GlobalPosition.DistanceTo(_targetDestinationPoint) < 5) {
			_smoothedPoints = new Queue<Vector2>(GenerateSmoothNavPoints(_navPoints.ToList()));
			if (_navPoints.Count > 0) {
				var target = _navPoints.Dequeue();
				_targetDestination = GetTargetPosition(target.Position);
			}

			if (_smoothedPoints.Count > 0) {
				_targetDestinationPoint = _smoothedPoints.Dequeue();
			}
		} else {
			var scale = Mathf.Clamp(1 - (GlobalPosition.DistanceTo(_targetDestinationPoint) / _jumpDistance), 0.1f, 1);
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
