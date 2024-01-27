using System.Collections.Generic;
using System.Linq;
using Godot;
using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.Entities;

public partial class Dreadnought : OffensiveUnitNode2D {
	private Node2D _canons;
	private List<Sprite2D> _cannonList;
	[Export] public PackedScene BulletScene;
	private Node _bulletContainer;

	public override void _Ready() {
		base._Ready();
		_bulletContainer = GetNode<Node>("Bullets");
		_canons = GetNode<Node2D>("Canons");
		_cannonList = _canons.GetChildren().Select(x => x as Sprite2D).ToList();
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (Target is null) {
			return;
		}

		foreach (var cannon in _cannonList) {
			cannon.LookAt(Target.Instance.GlobalPosition);
			cannon.Rotation += Mathf.Pi / 2;
		}
	}

	protected override void Shoot() {
		foreach (var cannon in _cannonList) {
			var marker = cannon.GetNode<Marker2D>("Marker");
			var bullet = BulletScene.Instantiate<SingleLaser>();
			bullet.GlobalPosition = marker.GlobalPosition;
			bullet.LookAt(Target.Instance.GlobalPosition);
			_bulletContainer.AddChild(bullet);
			bullet.BodyEntered += (body => HandleBulletBodyEntered(bullet, body));
		}
	}

	private void HandleBulletBodyEntered(SingleLaser bullet, Node2D body) {
		if (body != Target) {
			return;
		}
		((IAttack)this).Attack();
		bullet.QueueFree();
	}

}
