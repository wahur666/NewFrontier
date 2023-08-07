using Godot;
using System;
using NewFrontier.scripts;

public partial class BuildingNode2D : Node2D {
	[Export] public int Wide = 1;
	private int baseAngle = 30;
	private Planet _planet;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_planet = GetTree().CurrentScene.GetNode<Planet>("WetPlanet");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		// if (this.buildingShade) {
		// 	const npos = this.scene.getWorldPos(ev);
		// 	const distance = Phaser.Math.Distance.Between(npos.x, npos.y, this.scene.planet.x, this.scene.planet.y);
		// 	if (Math.abs(distance - this.scene.planet.radius) < 20) {
		// 		this.buildingShade.unBound = false;
		// 		this.buildingShade.nearPlanet = this.scene.planet;
		// 		const loc = this.buildingShade.calculatePlace(this.scene.planet, npos.x, npos.y);
		// 		console.log("loc", loc);
		// 	} else {
		// 		this.buildingShade.unBound = true;
		// 		this.buildingShade.nearPlanet = null;
		// 		this.buildingShade.setPosition(npos.x, npos.y);
		// 	}
		// }
		var pos = GetGlobalMousePosition();
		var place = CalculatePlace(_planet, pos.X, pos.Y);
		GD.Print(place.Join(", "));
	}

	public int[] CalculatePlace(Planet planet, double x, double y) {
		double angle = Math.Atan2(y - planet.GlobalPosition.Y, x - planet.GlobalPosition.X);
		double a = angle * (180 / Math.PI);
		double b = a > 0 ? a : 360 + a;
		int place = (int)((b + (Wide % 2 == 0 ? 0 : 15)) / 30) % 12;
		double diff = Wide % 2 == 0 ? baseAngle / 2 : 0;
		double posX = planet.GlobalPosition.X;
		double posY = planet.GlobalPosition.Y;
		RotationDegrees = place * 30 - 90 + (Wide % 2 == 0 ? 16 : 0);
		GlobalPosition = new Vector2((float)posX, (float)posY);
		GD.Print(GlobalPosition);

		int[] hoverPos;
		switch (Wide) {
			case 1:
				hoverPos = new[] { place };
				break;
			case 2: {
					int prev = place - 1 < 0 ? 11 : place - 1;
					int next = place > 11 ? 0 : place;
					hoverPos = new[] { prev, next };
					break;
				}
			case 3: {
					int prev = place - 1 < 0 ? 11 : place - 1;
					int next = place + 1 > 11 ? 0 : place + 1;
					hoverPos = new[] { prev, place, next };
					break;
				}
			default:
				hoverPos = Array.Empty<int>();
				break;
		}

		return hoverPos;
	}
}
