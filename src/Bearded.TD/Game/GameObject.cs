using System;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game
{
    abstract class GameObject : IDeletable, IGameObject
    {
        public GameState Game { get; private set; }

        public bool Deleted { get; private set; }
        public event VoidEventHandler Deleting;

        public void Add(GameState game)
        {
            if (game.ObjectBeingAdded != this || Game != null)
                throw new Exception("Bad!");
            Game = game;
            OnAdded();
        }

        protected virtual void OnAdded()
        {

        }

        protected void IsSingleton<T>()
            where T : class
        {
            var asT = this as T;
#if DEBUG
            if (asT == null)
                throw new Exception("Cannot list singleton as incompatible type");
#endif
            Game.RegisterSingleton(asT);
        }

        protected void ListAs<T>()
            where T : class, IDeletable
        {
            var asT = this as T;
#if DEBUG
            if (asT == null)
                throw new Exception("Cannot list as incompatible type");
#endif
            Game.ListAs(asT);
        }

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Draw(GeometryManager geometries);

        public void Delete()
        {
            OnDelete();
            Deleting?.Invoke();
            Deleted = true;
        }

        protected virtual void OnDelete()
        {

        }

    }
}
