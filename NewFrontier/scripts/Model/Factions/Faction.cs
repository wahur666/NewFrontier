using Godot;
using Godot.Collections;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.Entities;

namespace NewFrontier.scripts.Model.Factions;

public class Faction {
	public string Name;
	private Dictionary<string, PackedScene> _scenes = new();

	private Faction(string name) => Name = name;

	private void LoadScene(string name, string path) {
		var scene = GD.Load<PackedScene>(path);
		_scenes.Add(name, scene);
	}


	public static Faction CreateTerran() {
		var faction = new Faction(Factions.Terran);
		faction.InitTerran();
		return faction;
	}

	private void InitTerran() {
		LoadScene(Terran.Building1, "res://scenes/base_1.tscn");
		LoadScene(Terran.Building2, "res://scenes/base_2.tscn");
		LoadScene(Terran.Building3, "res://scenes/base_3.tscn");
		LoadScene(Terran.Jumpgate, "res://scenes/jumpgate.tscn");
	}

	public BuildingNode2D Create(PlayerController playerController, string name) {
		var instance = _scenes[name].Instantiate<BuildingNode2D>();
		if (name == Terran.Jumpgate) {
			instance.Wide = 2;
			instance.SnapToPlanet = SnapOption.Wormhole;
		}
		return instance.Init(playerController, name);
	}
}
