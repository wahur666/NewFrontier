using System;
using Godot;

namespace NewFrontier.scripts.Model.Interfaces;

public interface IBase {
	public int MaxHealth { protected set; get; }
	public int CurrentHealth { protected set; get; }

	public Node2D Instance { get; }

	public void Repair(int amount) {
		CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
	}

	public bool TakeDamage(int amount) {
		CurrentHealth = Math.Max(0, CurrentHealth - amount);
		if (CurrentHealth != 0) {
			return false;
		}

		Destroy();
		return true;

	}

	protected void Destroy();
}
