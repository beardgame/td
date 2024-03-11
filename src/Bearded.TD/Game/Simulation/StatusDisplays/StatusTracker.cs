using Bearded.TD.Game.Simulation.GameObjects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed partial class StatusTracker : Component, IStatusTracker
{
    public StatusTracker()
    {
        HitPointsBars = hitPointsBars.AsReadOnly();
        Statuses = statuses.AsReadOnly();
    }

    protected override void OnAdded() {}

    public override void Update(TimeSpan elapsedTime)
    {
        removedExpiredStatuses();
    }
}
