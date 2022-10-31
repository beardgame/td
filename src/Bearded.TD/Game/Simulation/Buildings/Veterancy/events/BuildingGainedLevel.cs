using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

readonly record struct BuildingGainedLevel(GameObject GameObject) : IGlobalEvent;
