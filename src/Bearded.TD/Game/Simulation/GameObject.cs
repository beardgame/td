using System;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation
{
    abstract class GameObject : IDeletable
    {
        private GameState? game;
        public GameState Game => game!;

        public bool Deleted { get; private set; }
        public event VoidEventHandler? Deleting;

        public void Add(GameState game)
        {
            if (game.ObjectBeingAdded != this || this.game != null)
            {
                throw new Exception("Tried adding game object to game in unexpected circumstances.");
            }

            this.game = game;
            OnAdded();
        }

        protected virtual void OnAdded() {}

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Draw(CoreDrawers geometries);

        public void Delete()
        {
            OnDelete();
            Deleting?.Invoke();
            Deleted = true;
        }

        protected virtual void OnDelete() {}
    }
}
