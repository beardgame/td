﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.UpdateLoop;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Simulation.World.Fluids;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation;

sealed class GameState
{
    private readonly Stack<GameObject> objectsBeingAdded = new();
    public GameObject? ObjectBeingAdded => objectsBeingAdded.Count == 0 ? null : objectsBeingAdded.Peek();

    private readonly DeletableObjectList<GameObject> gameObjects = new();
    private readonly Dictionary<Type, object> lists = new();
    private readonly Dictionary<Type, object> dictionaries = new();

    public EnumerableProxy<GameObject> GameObjects => gameObjects.AsReadOnlyEnumerable();

    public GameTime GameTime { get; } = new();
    public Instant Time => GameTime.Time;
    public GameMeta Meta { get; }
    public GameSettings GameSettings { get; }
    public Level Level { get; }
    public MultipleSinkNavigationSystem Navigator { get; }

    // Should only be used to communicate between game objects internally.
    public IdManager GamePlayIds { get; } = new();

    public GeometryLayer GeometryLayer { get; }
    public FluidLayer FluidLayer { get; }
    public UnitLayer UnitLayer { get; }
    public BuildingLayer BuildingLayer { get; }
    public BuildingPlacementLayer BuildingPlacementLayer { get; }
    public SelectionLayer SelectionLayer { get; }
    public PassabilityManager PassabilityManager { get; }
    public ZoneLayer ZoneLayer { get; }
    public VisibilityLayer VisibilityLayer { get; }

    public WaveDirector WaveDirector { get; }

    // TODO: this should be something managed per faction
    public ExplorationManager ExplorationManager { get; }

    private bool finishedLoading;

    private readonly GameFactions factions = new();
    public IGameFactions Factions { get; }

    public GameState(GameMeta meta, GameSettings gameSettings)
    {
        Meta = meta;
        GameSettings = gameSettings;
        Level = new Level(GameSettings.LevelSize);

        GeometryLayer = new GeometryLayer(Meta.Events, GameSettings.LevelSize);
        FluidLayer = new FluidLayer(this, GeometryLayer, GameSettings.LevelSize);
        UnitLayer = new UnitLayer();
        BuildingLayer = new BuildingLayer(Meta.Events);
        SelectionLayer = new SelectionLayer();
        PassabilityManager = new PassabilityManager(Meta.Events, Level, GeometryLayer, BuildingLayer);
        ZoneLayer = new ZoneLayer(GameSettings.LevelSize);
        VisibilityLayer = new VisibilityLayer(Meta.Events, ZoneLayer);
        BuildingPlacementLayer = new BuildingPlacementLayer(Level, GeometryLayer, BuildingLayer, VisibilityLayer,
            new Lazy<PassabilityLayer>(() => PassabilityManager!.GetLayer(Passability.WalkingUnit)));
        Navigator = new MultipleSinkNavigationSystem(Meta.Events, Level, PassabilityManager.GetLayer(Passability.WalkingUnit));
        Factions = factions.AsReadOnly();

        WaveDirector = new WaveDirector(this);
        ExplorationManager = new ExplorationManager(this);
    }

    public void FinishLoading()
    {
        validateLoadingCanBeFinished();

        Navigator.Initialize();
        finishedLoading = true;
    }

    private void validateLoadingCanBeFinished()
    {
        if (finishedLoading)
        {
            throw new Exception("Can only finish loading game state once.");
        }
    }

    public void Add(GameObject obj)
    {
        if (obj.Game != null)
        {
            throw new Exception("Sad!");
        }

        // ReSharper disable once HeuristicUnreachableCode
        // obj.Game is secretly nullable, but we want to spare ourselves ! operators in every single class
        gameObjects.Add(obj);
        objectsBeingAdded.Push(obj);
        obj.Add(this);
        var sameObj = objectsBeingAdded.Pop();
        DebugAssert.State.Satisfies(sameObj == obj);
        // event on added
    }

    public void ListAs<T>(T obj)
        where T : class, IDeletable
    {
        getList<T>().Add(obj);
    }

    public EnumerableProxy<T> Enumerate<T>()
        where T : class, IDeletable
        => getList<T>().AsReadOnlyEnumerable();

    public void IdAs<T>(Id<T> id, T obj)
    {
        var dict = getDictionary<T>();
        dict.Add(id, obj);
    }

    public bool DeleteId<T>(Id<T> id)
    {
        var dict = getDictionary<T>();
        return dict.Remove(id);
    }

    public T Find<T>(Id<T> id)
    {
        return getDictionary<T>()[id];
    }

    public bool TryFind<T>(Id<T> id, [NotNullWhen(true)] out T? result) where T : class
    {
        return getDictionary<T>().TryGetValue(id, out result);
    }

    private DeletableObjectList<T> getList<T>()
        where T : class, IDeletable
    {
        if (lists.TryGetValue(typeof(T), out var list))
        {
            return (DeletableObjectList<T>)list;
        }

        var l = new DeletableObjectList<T>();
        lists.Add(typeof(T), l);
        return l;
    }

    private Dictionary<Id<T>, T> getDictionary<T>()
    {
        if (dictionaries.TryGetValue(typeof(T), out var dict))
            return (Dictionary<Id<T>, T>)dict;

        var d = new Dictionary<Id<T>, T>();
        dictionaries.Add(typeof(T), d);
        return d;
    }

    public void AddFaction(Faction faction)
    {
        factions.Add(faction);
    }

    public void Advance(TimeSpan elapsedTime)
    {
        if (!finishedLoading)
        {
            throw new Exception("Must finish loading before advancing game state.");
        }

        GameTime.Advance(ref elapsedTime);

        FluidLayer.Update();
        WaveDirector.Update();

        foreach (var obj in gameObjects)
        {
            obj.Update(elapsedTime);
        }
    }
}
