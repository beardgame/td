using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed class Status(StatusSpec spec, Instant? expiryTime)
{
    public StatusType Type => spec.Type;
    public IStatusDrawer Drawer => spec.Drawer;
    public Instant? Expiry { get; set; } = expiryTime;
}
