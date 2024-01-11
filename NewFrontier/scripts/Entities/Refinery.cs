using Godot;

namespace NewFrontier.scripts.Entities;

public partial class Refinery : BuildingNode2D {
	public Marker2D BuildLocation;
	private Timer _oreTimer;
	private Timer _gasTimer;
	private int _baseMiningSpeed = 150;

	private const int TickRate = 25;
	private int _oreMiningLevel;
	private int _gasMiningLevel;

	public int OreMiningSpeed => _baseMiningSpeed + _oreMiningLevel * 30;
	public int GasMiningSpeed => _baseMiningSpeed + _gasMiningLevel * 30;

	public override void _Ready() {
		base._Ready();
		BuildLocation = GetNode<Marker2D>("BuildLocation");
		_oreTimer = GetNode<Timer>("OreTimer");
		_oreTimer.Timeout += OreMiningTimerTimeout;

		_gasTimer = GetNode<Timer>("GasTimer");
		_gasTimer.Timeout += GasMiningTimerTimeout;
	}

	public void StartTimers() {
		_oreTimer.WaitTime = 60 / (OreMiningSpeed / (double)TickRate);
		_oreTimer.Start();
		_gasTimer.WaitTime = 60 / (GasMiningSpeed / (double)TickRate);
		_gasTimer.Start();
	}

	private void OreMiningTimerTimeout() {
		if (Planet is null || PlayerController is null) {
			return;
		}

		var amount = Planet.PlanetStats.HarvestOre(TickRate);
		PlayerController.IncreaseOre(amount);
	}

	private void GasMiningTimerTimeout() {
		if (Planet is null || PlayerController is null) {
			return;
		}

		var amount = Planet.PlanetStats.HarvestGas(TickRate);
		PlayerController.IncreaseGas(amount);
	}
}
