using Bearded.TD.Game.Simulation.Events;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Factions;

abstract class FactionBehavior : IFactionBehavior
{
    [UsedImplicitly] // may be used in the future
    protected Faction Owner { get; private set; } = null!;
    protected GlobalGameEvents Events { get; private set; } = null!;

    public void OnAdded(Faction owner, GlobalGameEvents events)
    {
        Owner = owner;
        Events = events;
        Execute();
    }

    protected abstract void Execute();
}

[UsedImplicitly] // may be used in the future
abstract class FactionBehavior<TParameters> : FactionBehavior
{
    protected TParameters Parameters { get; }

    protected FactionBehavior(TParameters parameters)
    {
        Parameters = parameters;
    }
}
