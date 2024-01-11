using Godot;
using Godot.Collections;
using NewFrontier.scripts.Controllers;
using NewFrontier.scripts.Entities;
using NewFrontier.scripts.Model.Interfaces;

namespace NewFrontier.scripts.Model.Factions;

public class Faction {
	private readonly Dictionary<string, PackedScene> _scenes = new();
	public string Name;

	private Faction(string name) {
		Name = name;
	}

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
		LoadScene(Terran.Harvester, "res://scenes/harvester.tscn");
		LoadScene(Terran.Fabricator, "res://scenes/fabricator.tscn");
		LoadScene(Terran.Dreadnought, "res://scenes/dreadnought.tscn");
		LoadScene(Terran.Satellite, "res://scenes/satelite.tscn");
		LoadScene(Terran.IonCanon, "res://scenes/ion_canon.tscn");
		LoadScene(Terran.Refinery, "res://scenes/refinery.tscn");
	}

	private IBuildable CreateBuilding(PlayerController playerController, string name) {
		var instance = _scenes[name].Instantiate<BuildingNode2D>();
		instance.Init(playerController, name);
		return instance;
	}

	public IBuildable CreateBuilding1(PlayerController playerController) {
		return CreateBuilding(playerController, Terran.Building1);
	}

	public IBuildable CreateBuilding2(PlayerController playerController) {
		return CreateBuilding(playerController, Terran.Building2);
	}

	public IBuildable CreateBuilding3(PlayerController playerController) {
		return CreateBuilding(playerController, Terran.Building3);
	}
	
	public IBuildable CreateRefinery(PlayerController playerController) {
		return CreateBuilding(playerController, Terran.Refinery);
	}

	public IBuildable CreateJumpgate(PlayerController playerController) {
		return CreateBuilding(playerController, Terran.Jumpgate);
	}

	public IBuildable CreateIonCanon(PlayerController playerController) {
		return CreateBuilding(playerController, Terran.IonCanon);
	}

	public IBuildable CreateSatellite(PlayerController playerController) {
		return CreateBuilding(playerController, Terran.Satellite);
	}

	private T CreateUnit<T>(PlayerController playerController, string name, Vector2 position,
		UiController uiController) where T : UnitNode2D {
		var instance = _scenes[name].Instantiate<T>();
		instance.Init(position, playerController, uiController);
		return instance;
	}

	public Harvester CreateHarvester(Vector2 position, PlayerController playerController,
		UiController uiController) {
		return CreateUnit<Harvester>(playerController, Terran.Harvester, position, uiController);
	}

	public Fabricator CreateFabricator(Vector2 position, PlayerController playerController,
		UiController uiController) {
		return CreateUnit<Fabricator>(playerController, Terran.Fabricator, position, uiController);
	}

	public Dreadnought CreateDreadnought(Vector2 position, PlayerController playerController,
		UiController uiController) {
		return CreateUnit<Dreadnought>(playerController, Terran.Dreadnought, position, uiController);
	}
}
