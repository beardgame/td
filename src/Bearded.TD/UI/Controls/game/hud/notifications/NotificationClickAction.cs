using Bearded.TD.Game;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

delegate void NotificationClickAction();

static class NotificationClickActionFactory
{
    public static NotificationClickAction ScrollTo(GameInstance game, IPositionable positionable) =>
        () => game.CameraController.ScrollToWorldPos(positionable.Position.XY());
}
