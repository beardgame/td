using System;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Statistics
{
    sealed class StatisticCollector<T> : IComponent<T>
    {
        private long totalDamage;
        private long totalKills;

        public void OnAdded(T owner, ComponentEvents events)
        {
            events.Subscribe(Listener.ForEvent<CausedDamage>(e => totalDamage += e.Result.DamageTaken));
            events.Subscribe(Listener.ForEvent<CausedKill>(_ => totalKills++));
        }

        public void Update(TimeSpan elapsedTime) {}

        public void Draw(CoreDrawers drawers) {}

        public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => false;

        public void ApplyUpgradeEffect(IUpgradeEffect effect) => throw new InvalidOperationException();

        public bool RemoveUpgradeEffect(IUpgradeEffect effect) => throw new InvalidOperationException();
    }
}
