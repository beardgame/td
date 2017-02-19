using amulware.Graphics;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    abstract class Component
    {
        public Building Building { get; private set; }

        public void OnAdded(Building building)
        {
            Building = building;
            Initialise();
        }

        protected abstract void Initialise();

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Draw();
    }
}