using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

record struct FindObjectRuinState(bool IsRuined) : IComponentPreviewEvent;
