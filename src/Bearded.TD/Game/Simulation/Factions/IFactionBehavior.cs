using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Factions
{
    abstract class FactionBehavior<TOwner>
    {
        protected TOwner Owner { get; private set; }
        protected GlobalGameEvents Events { get; private set; } = null!;

        public void OnAdded(TOwner owner, GlobalGameEvents events)
        {
            Owner = owner;
            Events = events;
            Execute();
        }

        protected abstract void Execute();
    }
}
