﻿using Godot;
using Godot.Collections;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.Entities;

namespace NewFrontier.scripts.Model.Factions;

public class Faction {
	public string Name;
	public Dictionary<string, PackedScene> Scenes = new();

	private Faction(string name) {
		Name = name;
	}

	private void LoadScene(string name, string path) {
		var scene = GD.Load<PackedScene>(path);
		Scenes.Add(name, scene);
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
		var instance = Scenes[name].Instantiate<BuildingNode2D>();
		if (name == Terran.Jumpgate) {
			instance.SnapToPlanet = SnapOption.Wormhole;
		}
		return instance.Init(playerController, name);
	}
}
