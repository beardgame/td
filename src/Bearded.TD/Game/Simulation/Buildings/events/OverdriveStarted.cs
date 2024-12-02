using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings;

[Trigger("overdriveStarted")]
readonly record struct OverdriveStarted : IComponentEvent;
