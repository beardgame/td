using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Factions
{
    sealed class OwnedByFaction<T> : Component<T>, IOwnedByFaction
    {
        public Faction Faction { get; }

        public OwnedByFaction(Faction faction)
        {
            Faction = faction;
        }

        protected override void Initialize() {}
        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}
    }

    interface IOwnedByFaction
    {
        public Faction Faction { get; }
    }
}
