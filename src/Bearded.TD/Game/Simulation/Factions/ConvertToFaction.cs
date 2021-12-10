using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Factions
{
    readonly record struct ConvertToFaction(Faction Faction) : IComponentEvent;
}
