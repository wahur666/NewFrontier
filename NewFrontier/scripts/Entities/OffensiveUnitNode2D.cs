using System;
using Godot;
using NewFrontier.scripts.Model;
using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.Entities;

public partial class OffensiveUnitNode2D : UnitNode2D, IAttack {
	[ExportGroup("OffensiveStats")] [Export]
	public int CurrentSupply;

	[ExportGroup("OffensiveStats")] [Export]
	public int Damage;

	[ExportGroup("OffensiveStats")] [Export]
	public int MaxSupply;

	private Timer _attackTimer;

	public float AttackRange { get; set; } = 200f;
	public IBase Target { get; set; }


	private bool _weaponOnCooldown = false;

	public override void _Ready() {
		base._Ready();
		_attackTimer = GetNode<Timer>("AttackTimer");
		_attackTimer.Timeout += () => _weaponOnCooldown = false;
	}

	public int CalculateDamage() {
		return Damage;
	}

	protected virtual void Shoot() {
		throw new Exception("Implement Shoot!");
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (Target is not null) {
			var d = Target.Instance.Position.DistanceTo(Position);
			if (d < AttackRange && !_weaponOnCooldown) {
				Shoot();
				_attackTimer.Start();
				_weaponOnCooldown = true;
			} else {
				GD.Print($"Distance: {d}");
			}
		}

		
	}
}
