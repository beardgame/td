using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Events;

sealed class GlobalGameEvents : GameEvents<IGlobalEvent, IGlobalPreviewEvent> {}