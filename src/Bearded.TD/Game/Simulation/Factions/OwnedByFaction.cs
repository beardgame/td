using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Factions
{
    sealed class FactionProvider<T> : Component<T>, IFactionProvider
    {
        public Faction Faction { get; }

        public FactionProvider(Faction faction)
        {
            Faction = faction;
        }

        protected override void OnAdded() {}
        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}
    }

    interface IFactionProvider
    {
        public Faction Faction { get; }
    }
}
