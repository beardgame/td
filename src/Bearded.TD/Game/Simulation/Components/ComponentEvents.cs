using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Components;

sealed class ComponentEvents : GameEvents<IComponentEvent, IComponentPreviewEvent> {}