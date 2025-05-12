using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Simulation.Damage;

partial class HitPointsPool
{
    public IStateToSync GetCurrentStateToSync() => new HealthSynchronizedState(this);

    private sealed class HealthSynchronizedState(HitPointsPool source) : IStateToSync
    {
        private float currentHealth = source.CurrentHitPoints.NumericValue;

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref currentHealth);
        }

        public void Apply()
        {
            source.OverrideCurrentHitPoints(currentHealth.HitPoints());
        }
    }
}
