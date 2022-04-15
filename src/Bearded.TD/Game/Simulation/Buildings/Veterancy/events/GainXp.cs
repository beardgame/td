using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

readonly record struct GainXp(Experience Amount) : IComponentEvent;
