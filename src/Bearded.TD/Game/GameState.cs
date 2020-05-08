using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Rules;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Workers;
using Bearded.TD.Game.World;
using Bearded.TD.Game.World.Fluids;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game
{
    [GameRuleOwner]
    sealed class GameState
    {
        private readonly Stack<GameObject> objectsBeingAdded = new Stack<GameObject>();
        public GameObject ObjectBeingAdded => objectsBeingAdded.Count == 0 ? null : objectsBeingAdded.Peek();

        private readonly DeletableObjectList<GameObject> gameObjects = new DeletableObjectList<GameObject>();
        private readonly List<IGameRule<GameState>> gameRules = new List<IGameRule<GameState>>();
        private readonly Dictionary<Type, object> lists = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> dictionaries = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> singletons = new Dictionary<Type, object>();

        public EnumerableProxy<GameObject> GameObjects => gameObjects.AsReadOnlyEnumerable();
        public ReadOnlyCollection<IGameRule<GameState>> Rules;

        public Instant Time { get; private set; } = Instant.Zero;
        public GameMeta Meta { get; }
        public GameSettings GameSettings { get; }
        public Level Level { get; }
        public MultipleSinkNavigationSystem Navigator { get; }

        // Should only be used to communicate between game objects internally.
        public IdManager GamePlayIds { get; } = new IdManager();

        public GeometryLayer GeometryLayer { get; }
        public FluidLayer FluidLayer { get; }
        public UnitLayer UnitLayer { get; }
        public BuildingLayer BuildingLayer { get; }
        public BuildingPlacementLayer BuildingPlacementLayer { get; }
        public MiningLayer MiningLayer { get; }
        public PassabilityManager PassabilityManager { get; }

        private bool isLoading = true;

        private readonly IdCollection<Faction> factions = new IdCollection<Faction>();
        public ReadOnlyCollection<Faction> Factions => factions.AsReadOnly;
        public Faction RootFaction { get; private set; }

        public GameState(GameMeta meta, GameSettings gameSettings)
        {
            Meta = meta;
            GameSettings = gameSettings;
            Level = new Level(GameSettings.LevelSize);
            Rules = gameRules.AsReadOnly();

            GeometryLayer = new GeometryLayer(Meta.Events, GameSettings.LevelSize);
            FluidLayer = new FluidLayer(this, GeometryLayer, GameSettings.LevelSize);
            UnitLayer = new UnitLayer();
            BuildingLayer = new BuildingLayer(Meta.Events);
            BuildingPlacementLayer = new BuildingPlacementLayer(Level, GeometryLayer, BuildingLayer,
                new Lazy<PassabilityLayer>(() => PassabilityManager.GetLayer(Passability.WalkingUnit)));
            MiningLayer = new MiningLayer(Meta.Logger, Meta.Events, Level, GeometryLayer);
            PassabilityManager = new PassabilityManager(Meta.Events, Level, GeometryLayer, BuildingLayer);
            Navigator = new MultipleSinkNavigationSystem(Meta.Events, Level, PassabilityManager.GetLayer(Passability.WalkingUnit));
        }

        public void FinishLoading()
        {
            if (!isLoading)
            {
                throw new Exception("Can only finish loading game state once.");
            }

            Navigator.Initialise();
            isLoading = false;
        }

        public void Add(GameObject obj)
        {
            if (obj.Game != null)
            {
                throw new Exception("Sad!");
            }

            gameObjects.Add(obj);
            objectsBeingAdded.Push(obj);
            obj.Add(this);
            var sameObj = objectsBeingAdded.Pop();
            DebugAssert.State.Satisfies(sameObj == obj);
            // event on added
        }

        public void Add(IGameRule<GameState> rule)
        {
            gameRules.Add(rule);
            rule.OnAdded(this, Meta.Events);
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
            return (T)obj;
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
            ListAs(obj);
            var dict = getDictionary<T>();
            dict.Add(obj);
            obj.Deleting += () => dict.Remove(obj);
        }

        public T Find<T>(Id<T> id)
            where T : class, IIdable<T>
        {
            return getDictionary<T>()[id];
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

        public Faction FactionFor(Id<Faction> id) => factions[id];

        public void AddFaction(Faction faction)
        {
            factions.Add(faction);
            if (faction.Parent != null) return;

            if (RootFaction != null)
            {
                throw new Exception("Can only have one root faction. All other factions need parents.");
            }

            RootFaction = faction;
        }

        public void Advance(TimeSpan elapsedTime)
        {
            if (isLoading)
            {
                throw new Exception("Must finish loading before advancing game state.");
            }

            Time += elapsedTime;

            FluidLayer.Update();

            foreach (var rule in gameRules)
            {
                rule.Update(elapsedTime);
            }

            foreach (var obj in gameObjects)
            {
                obj.Update(elapsedTime);
            }
        }
    }
}
