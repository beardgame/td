using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

record struct FindObjectRuinState(bool IsRuined) : IComponentPreviewEvent;
