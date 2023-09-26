using System;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

static class StatusDrawerExtensions
{
    public static IStatusDrawer WithProgress(this IStatusDrawer inner, Func<float> getProgress) =>
        new ProgressStatusDrawer(inner, getProgress);
}
