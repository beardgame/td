using System.Collections.Generic;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Controls;

delegate void NotificationClickAction();

static class NotificationClickActionFactory
{
    public static NotificationClickAction ScrollTo(GameInstance game, IPositionable positionable) =>
        () => game.CameraController.ScrollToWorldPos(positionable.Position.XY());

    public static NotificationClickAction ScrollToContain(GameInstance game, IEnumerable<IPositionable> positionables)
    {
        var min = new Position2(float.MaxValue, float.MaxValue);
        var max = new Position2(float.MinValue, float.MinValue);
        foreach (var positionable in positionables)
        {
            min = new Position2(Unit.Min(min.X, positionable.Position.X), Unit.Min(min.Y, positionable.Position.Y));
            max = new Position2(Unit.Max(max.X, positionable.Position.X), Unit.Max(max.Y, positionable.Position.Y));
        }

        if (min.X > max.X)
        {
            return () => { };
        }

        return () => game.CameraController.ScrollToBoundingBox(min, max);
    }
}
