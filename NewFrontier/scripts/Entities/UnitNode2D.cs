using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.helpers;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.UI;

namespace NewFrontier.scripts.Entities;

public partial class UnitNode2D : CharacterBody2D {
	private bool _selected;
	private int _speed = 300;
	private TravelState _travelState = TravelState.NotTraveling;

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

	public Queue<GameNode> navPpoints = new();
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
		points.AddRange(navPpoints.Select(x => MapHelpers.GridCoordToGridCenterPos(x.Position)));
		DrawPath(points);
	}

	public bool InsideSelectionRect(Vector2 position) {
		var size = SelectionRect.Size;
		return AreaHelper.InRect(position, Position + size * new Vector2(.5f, .5f),
			Position - size * new Vector2(.5f, .5f));
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
		
		if (Position.DistanceTo(TargetDestination) < 5) {
			if (navPpoints.Count > 0) {
				_moving = true;
				TargetDestination = MapHelpers.GridCoordToGridCenterPos(navPpoints.Dequeue().Position);
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
			targetRotation -= Mathf.Tau;
		}

		while (targetRotation - Rotation < -Mathf.Pi) {
			targetRotation += Mathf.Tau;
		}

		// Define a constant rotation speed
		float rotationSpeed = 2.0f; // You can adjust this value as needed

		// Calculate the rotation difference
		float rotationDifference = targetRotation - Rotation + Mathf.Pi / 2;

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

		const int squareSize = MapHelpers.DrawSize / 4;
		const int halfSquareSize = squareSize / 2;
		canvas.DrawRect(
			new Rect2(path[^1] - new Vector2(halfSquareSize, halfSquareSize),
				new Vector2(squareSize, squareSize)),
			Colors.Aqua);
		canvas.QueueRedraw();
	}

	public void SetNavigation(List<GameNode> vector2s) {
		TargetDestination = Position;
		if (vector2s.Count > 1) {
			vector2s.RemoveAt(0);
		}

		this.navPpoints = new Queue<GameNode>(vector2s);
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
