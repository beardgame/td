using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.Factions
{
    interface IFactionBlueprint : IBlueprint
    {
        IEnumerable<FactionBehavior<Faction>> GetBehaviors();
    }
}
