using System;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed class Status(StatusSpec spec, Instant? expiryTime)
{
    public StatusType Type => spec.Type;
    [Obsolete] public IStatusDrawer Drawer => spec.Drawer;
    public IStatusDrawSpec DrawSpec => spec.DrawSpec;
    public Instant? Expiry { get; set; } = expiryTime;
}
