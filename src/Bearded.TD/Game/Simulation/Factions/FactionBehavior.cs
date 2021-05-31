using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Factions
{
    abstract class FactionBehavior<TOwner> : IFactionBehavior<TOwner>
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

    abstract class FactionBehavior<TOwner, TParameters> : FactionBehavior<TOwner>
    {
        protected TParameters Parameters { get; }

        protected FactionBehavior(TParameters parameters)
        {
            Parameters = parameters;
        }
    }
}
