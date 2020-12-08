using System.Collections.Generic;

namespace Bearded.TD.Game.GameState.Technologies
{
    interface ITechnologyBlueprint : IBlueprint
    {
        string Name { get; }
        int Cost { get; }
        IEnumerable<ITechnologyUnlock> Unlocks { get; }
        IEnumerable<ITechnologyBlueprint> RequiredTechs { get; }
    }
}
