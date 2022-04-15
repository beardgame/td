using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.Technologies;

interface ITechnologyBlueprint : IBlueprint
{
    string Name { get; }
    IEnumerable<ITechnologyUnlock> Unlocks { get; }
    IEnumerable<ITechnologyBlueprint> RequiredTechs { get; }
}
