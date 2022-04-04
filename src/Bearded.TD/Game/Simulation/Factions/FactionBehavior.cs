using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Factions;

abstract class FactionBehavior : IFactionBehavior
{
    protected Faction Owner { get; private set; }
    protected GlobalGameEvents Events { get; private set; } = null!;

    public void OnAdded(Faction owner, GlobalGameEvents events)
    {
        Owner = owner;
        Events = events;
        Execute();
    }

    protected abstract void Execute();
}

abstract class FactionBehavior<TParameters> : FactionBehavior
{
    protected TParameters Parameters { get; }

    protected FactionBehavior(TParameters parameters)
    {
        Parameters = parameters;
    }
}
