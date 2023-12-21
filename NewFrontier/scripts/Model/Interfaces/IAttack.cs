using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.Model; 

public interface IAttack {

	protected int CalculateDamage(IBase target);
	public void Attack(IBase target) {
		target.TakeDamage(CalculateDamage(target));
	}
}
