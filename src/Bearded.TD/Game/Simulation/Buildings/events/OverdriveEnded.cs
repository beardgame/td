using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings;

[Trigger("overdriveEnded")]
readonly record struct OverdriveEnded : IComponentEvent;
