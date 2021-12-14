using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

readonly record struct RepairFinished(Faction RepairingFaction) : IComponentEvent;