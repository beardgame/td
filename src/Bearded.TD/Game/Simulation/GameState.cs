using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Simulation.World.Fluids;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation
{
    [GameRuleOwner]
    sealed class GameState : ITimeSource
    {
        private readonly Stack<GameObject> objectsBeingAdded = new();
        public GameObject? ObjectBeingAdded => objectsBeingAdded.Count == 0 ? null : objectsBeingAdded.Peek();

        private readonly DeletableObjectList<GameObject> gameObjects = new();
        private readonly Dictionary<Type, object> lists = new();
        private readonly Dictionary<Type, object> dictionaries = new();
        private readonly Dictionary<Type, object> singletons = new();

        public EnumerableProxy<GameObject> GameObjects => gameObjects.AsReadOnlyEnumerable();

        public Instant Time { get; private set; } = Instant.Zero;
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
        public MiningLayer MiningLayer { get; }
        public SelectionLayer SelectionLayer { get; }
        public PassabilityManager PassabilityManager { get; }

        public WaveDirector WaveDirector { get; }

        private bool finishedLoading;

        public GameFactions Factions { get; }

        private Faction? rootFaction;
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        // Separate nullable field for lazy initialisation in AddFaction. Needs to be set during loading.
        public Faction RootFaction => rootFaction!;

        public GameState(GameMeta meta, GameSettings gameSettings)
        {
            Meta = meta;
            GameSettings = gameSettings;
            Level = new Level(GameSettings.LevelSize);

            GeometryLayer = new GeometryLayer(Meta.Events, GameSettings.LevelSize);
            FluidLayer = new FluidLayer(this, GeometryLayer, GameSettings.LevelSize);
            UnitLayer = new UnitLayer();
            BuildingLayer = new BuildingLayer(Meta.Events);
            BuildingPlacementLayer = new BuildingPlacementLayer(Level, GeometryLayer, BuildingLayer,
                new Lazy<PassabilityLayer>(() => PassabilityManager.GetLayer(Passability.WalkingUnit)));
            MiningLayer = new MiningLayer(Meta.Logger, Meta.Events, Level, GeometryLayer);
            SelectionLayer = new SelectionLayer(BuildingLayer);
            PassabilityManager = new PassabilityManager(Meta.Events, Level, GeometryLayer, BuildingLayer);
            Navigator = new MultipleSinkNavigationSystem(Meta.Events, Level, PassabilityManager.GetLayer(Passability.WalkingUnit));
            Factions = new GameFactions();

            WaveDirector = new WaveDirector(this);
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

            if (rootFaction == null)
            {
                throw new InvalidOperationException("Root faction must be set during game loading.");
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

        public void RegisterSingleton<T>(T obj)
            where T : class
        {
            if (obj != ObjectBeingAdded)
                throw new Exception("Sad!");

            var type = typeof(T);
            if (singletons.ContainsKey(type))
                throw new Exception($"Can only instantiate one {type.Name} per game.");

            singletons.Add(type, obj);
        }

        public T Get<T>()
            where T : class
        {
            singletons.TryGetValue(typeof(T), out var obj);
            return (T)obj ?? throw new InvalidOperationException($"Singleton {typeof(T)} not found.");
        }

        public void ListAs<T>(T obj)
            where T : class, IDeletable
        {
            if (obj != ObjectBeingAdded)
            {
                throw new Exception("Sad!");
            }

            getList<T>().Add(obj);
        }

        public EnumerableProxy<T> Enumerate<T>()
            where T : class, IDeletable
            => getList<T>().AsReadOnlyEnumerable();

        public void IdAs<T>(T obj)
            where T : GameObject, IIdable<T>
        {
            var dict = getDictionary<T>();
            dict.Add(obj);
            obj.Deleting += () => dict.Remove(obj);
        }

        public T Find<T>(Id<T> id)
            where T : class, IIdable<T>
        {
            return getDictionary<T>()[id];
        }

        public bool TryFind<T>(Id<T> id, [NotNullWhen(true)] out T? result) where T : class, IIdable<T>
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

        private IdDictionary<T> getDictionary<T>()
            where T : class, IIdable<T>
        {
            if (dictionaries.TryGetValue(typeof(T), out var dict))
                return (IdDictionary<T>)dict;

            var d = new IdDictionary<T>();
            dictionaries.Add(typeof(T), d);
            return d;
        }

        public void AddFaction(Faction faction)
        {
            if (faction.Parent == null)
            {
                if (rootFaction != null)
                {
                    throw new Exception("Can only have one root faction. All other factions need parents.");
                }

                rootFaction = faction;
            }

            Factions.Add(faction);
        }

        public void Advance(TimeSpan elapsedTime)
        {
            if (!finishedLoading)
            {
                throw new Exception("Must finish loading before advancing game state.");
            }

            Time += elapsedTime;

            FluidLayer.Update();
            WaveDirector.Update();

            foreach (var obj in gameObjects)
            {
                obj.Update(elapsedTime);
            }
        }
    }
}
