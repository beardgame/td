using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

[Trigger("levelGained")]
readonly record struct GainLevel : IComponentEvent;
