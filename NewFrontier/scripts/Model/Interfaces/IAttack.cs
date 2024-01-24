namespace NewFrontier.scripts.Model.Interfaces;

public interface IAttack {

	public IBase Target { get; set; }
	
	protected int CalculateDamage();

	public void Attack() {
		var free = Target.TakeDamage(CalculateDamage());
		if (free) Target = null;
	}
}
