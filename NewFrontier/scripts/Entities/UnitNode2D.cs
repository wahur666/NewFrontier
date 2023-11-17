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

	private Queue<GameNode> _navPoints = new();
	private PlayerController _playerController;

	private SelectionRect _selectionRect;

	private Vector2 _targetDestination;
	private UiController _uiController;

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

		// switch (_travelState) {
		// 	case TravelState.PrepareForTraveling:
		// 		break;
		// 	case TravelState.Traveling:
		// 		break;
		// 	case TravelState.EndTraveling:
		// 		break;
		// 	case TravelState.NotTraveling:
		// 	default:
		// 		throw new ArgumentOutOfRangeException();
		// }

		if (Position.DistanceTo(_targetDestination) < 5) {
			if (_navPoints.Count > 0) {
				_moving = true;
				_targetDestination = MapHelpers.GridCoordToGridCenterPos(_navPoints.Dequeue().Position);
			} else {
				_moving = false;
				Velocity = Vector2.Zero;
			}

			return;
		}

		var direction = _targetDestination - GlobalPosition;
		direction = direction.Normalized();

		// Calculate the angle between the current rotation and the direction vector
		var targetRotation = Mathf.Atan2(direction.Y, direction.X);

		// Ensure the angle is between -PI and PI
		while (targetRotation - Rotation > Mathf.Pi) {
			targetRotation -= Mathf.Tau;
		}

		while (targetRotation - Rotation < -Mathf.Pi) {
			targetRotation += Mathf.Tau;
		}

		// Define a constant rotation speed
		var rotationSpeed = 2.0f; // You can adjust this value as needed

		// Calculate the rotation difference
		var rotationDifference = targetRotation - Rotation + (Mathf.Pi / 2);

		// Determine the shortest path to the target rotation
		if (rotationDifference > Mathf.Pi) {
			rotationDifference -= Mathf.Tau;
		} else if (rotationDifference < -Mathf.Pi) {
			rotationDifference += Mathf.Tau;
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
		_targetDestination = Position;
		if (vector2S.Count > 1) {
			vector2S.RemoveAt(0);
		}

		_navPoints = new Queue<GameNode>(vector2S);
	}

	// updateNotTraveling(delta: number) {
	//        if (this.selected) {
	//            this.drawPath();
	//        }
	//        if (this.navNodes.value.length > 0) {
	//            const target = this.navPoints.value[0];
	//            if (this.pos.distance(target) < 5) {
	//                this.navNodes.value = this.navNodes.value.slice(1);
	//                if (this.navNodes.value.length === 0) {
	//                    this.stopNav();
	//                } else if (this.navNodes.value.length > 1 && this.navNodes.value[0].hasWormhole && this.navNodes.value[1].hasWormhole) {
	//                    this.traveling = TravelState.PREPARE_FOR_TRAVELING;
	//                }
	//            } else {
	//                this.moveToTarget(target, 50);
	//            }
	//        }
	//    }
	//
	//    updatePrepareForTraveling(delta: number) {
	//        // Wait until everybody is read for jump
	//        this.selectedGraphics.clear();
	//        this.selectedGraphics.lineStyle(2, 0x00ff00, 1);
	//        this.selectedGraphics.lineBetween(this.x, this.y, this.navPoints.value[0].x, this.navPoints.value[0].y);
	//        this.body?.stop();
	//        const target = this.navPoints.value[0];
	//        this.setRotation(Math.atan2(-this.y + target.y, -this.x + target.x) + Math.PI / 2);
	//        if (this.currentTravelTime < this.travelTime) {
	//            this.currentTravelTime += delta;
	//            return;
	//        }
	//        if (this.pos.distance(target) < 5) {
	//            this.navNodes.value = this.navNodes.value.slice(1);
	//        } else {
	//            this.moveToTarget(target, 200);
	//            return;
	//        }
	//        this.currentTravelTime = 0;
	//        this.traveling = TravelState.TRAVELING;
	//    }
	//
	//    updateTraveling(delta: number) {
	//        this.visible = false;
	//        if (this.currentTravelTime < this.travelTime) {
	//            this.currentTravelTime += delta;
	//            return;
	//        }
	//        const target = this.navPoints.value[0];
	//        this.x = target.x;
	//        this.y = target.y;
	//        this.currentTravelTime = 0;
	//        this.navNodes.value = this.navNodes.value.slice(1);
	//        this.traveling = TravelState.END_TRAVELING;
	//    }
	//
	//    private void UpdateEndTraveling(double delta) {
	//        Visible = true;
	//        // speed up ship until its reached the next position
	//        var target = this.navPoints.value[0];
	//        if (this.pos.distance(target) < 5) {
	//            this.navNodes.value = this.navNodes.value.slice(1);
	//        } else {
	//            this.moveToTarget(target, 200);
	//            return;
	//        }
	//        if (this.navNodes.value.length === 0) {
	//            this.stopNav();
	//        }
	//        this.traveling = TravelState.NOT_TRAVELING;
	//    }
}
