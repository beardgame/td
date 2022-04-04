using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Factions;

readonly record struct ConvertToFaction(Faction Faction) : IComponentEvent;
