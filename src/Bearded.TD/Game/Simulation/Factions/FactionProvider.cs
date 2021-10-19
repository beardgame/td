using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Factions
{
    sealed class FactionProvider<T> : Component<T>, IListener<ConvertToFaction>, IFactionProvider
    {
        public Faction Faction { get; private set; }

        public FactionProvider(Faction faction)
        {
            Faction = faction;
        }

        protected override void OnAdded()
        {
            Events.Subscribe(this);
        }

        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}

        public void HandleEvent(ConvertToFaction @event)
        {
            Faction = @event.Faction;
        }
    }

    interface IFactionProvider
    {
        public Faction Faction { get; }
    }
}
