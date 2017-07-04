using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game
{
    class GameState
    {
        public GameObject ObjectBeingAdded { get; private set; }

        private readonly DeletableObjectList<GameObject> gameObjects = new DeletableObjectList<GameObject>();
        private readonly Dictionary<Type, object> lists = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> dictionaries = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> singletons = new Dictionary<Type, object>();

        public EnumerableProxy<GameObject> GameObjects => gameObjects.AsReadOnlyEnumerable();

        public Instant Time { get; private set; } = Instant.Zero;
        public GameMeta Meta { get; }
        public Level Level { get; }
        public LevelGeometry Geometry { get; }
        public MultipleSinkNavigationSystem Navigator { get; }

        public bool IsLoading { get; private set; } = true;

        private readonly IdCollection<Faction> factions = new IdCollection<Faction>();
        public ReadOnlyCollection<Faction> Factions => factions.AsReadOnly;
        public Faction RootFaction { get; private set; }

        public GameState(GameMeta meta, Level level)
        {
            Meta = meta;
            Level = level;
            Geometry = new LevelGeometry(level.Tilemap);
            Navigator = new MultipleSinkNavigationSystem(Geometry);
        }

        public void FinishLoading()
        {
            if (!IsLoading)
                throw new Exception("Can only finish loading game state once.");

            Geometry.Initialise();
            Navigator.Initialise(Geometry);
            IsLoading = false;
        }

        public void Add(GameObject obj)
        {
            if (obj.Game != null)
                throw new Exception("Sad!");

            gameObjects.Add(obj);
            ObjectBeingAdded = obj;
            obj.Add(this);
            ObjectBeingAdded = null;
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
            object obj;
            singletons.TryGetValue(typeof(T), out obj);
            return (T)obj;
        }

        public void ListAs<T>(T obj)
            where T : class, IDeletable
        {
            if (obj != ObjectBeingAdded)
                throw new Exception("Sad!");

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
            object list;
            if (lists.TryGetValue(typeof(T), out list))
                return (DeletableObjectList<T>)list;

            var l = new DeletableObjectList<T>();
            lists.Add(typeof(T), l);
            return l;
        }


        private IdDictionary<T> getDictionary<T>()
            where T : class, IIdable<T>
        {
            object dict;
            if (dictionaries.TryGetValue(typeof(T), out dict))
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
                throw new Exception("Can only have one root faction. All other factions need parents.");
            RootFaction = faction;
        }

        public void RemoveFaction(Faction faction)
        {
            factions.Remove(faction);
        }

        public void Advance(TimeSpan elapsedTime)
        {
            if (IsLoading)
                throw new Exception("Must finish loading before advancing game state.");

            Time += elapsedTime;

            foreach (var obj in gameObjects)
            {
                obj.Update(elapsedTime);
            }
        }
    }
}
