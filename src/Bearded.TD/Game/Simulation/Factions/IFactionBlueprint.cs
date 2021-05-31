using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game.Simulation.Factions
{
    interface IFactionBlueprint : IBlueprint
    {
        new ExternalId<Faction> Id { get; }
        string? Name { get; }
        Color? Color { get; }

        IEnumerable<IFactionBehavior<Faction>> GetBehaviors();
    }
}
