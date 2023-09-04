using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Simulation.Damage;

abstract partial class HitPointsPool<T>
{
    public IStateToSync GetCurrentStateToSync() => new HealthSynchronizedState(this);

    private sealed class HealthSynchronizedState : IStateToSync
    {
        private readonly HitPointsPool<T> source;
        private float currentHealth;

        public HealthSynchronizedState(HitPointsPool<T> source)
        {
            this.source = source;
            currentHealth = source.CurrentHitPoints.NumericValue;
        }

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
