using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

readonly record struct RepairFinished(Faction RepairingFaction) : IComponentEvent;
