using System;

namespace NewFrontier.scripts.Model.Interfaces;

public interface IBase {
	public int MaxHealth { protected set; get; }
	public int CurrentHealth { protected set; get; }

	public void Repair(int amount) {
		CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
	}

	public void TakeDamage(int amount) {
		CurrentHealth = Math.Max(0, CurrentHealth - amount);
		if (CurrentHealth == 0) {
			Destroy();
		}
	}

	protected void Destroy();
}
