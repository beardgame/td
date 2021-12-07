using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class DebugInvulnerable<T> : Component<T>, IPreviewListener<PreviewTakeDamage>
    {
        protected override void OnAdded()
        {
            Events.Subscribe(this);
        }

        public override void Update(TimeSpan elapsedTime) {}

        public void PreviewEvent(ref PreviewTakeDamage @event)
        {
            if (UserSettings.Instance.Debug.InvulnerableBuildings)
            {
                @event = @event.CappedAt(HitPoints.Zero);
            }
        }
    }
}
