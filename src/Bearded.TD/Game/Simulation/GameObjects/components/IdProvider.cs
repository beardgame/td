using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects
{
    sealed class IdProvider<T> : Component<T>, IIdProvider<T>
    {
        public Id<T> Id { get; }

        public IdProvider(Id<T> id)
        {
            Id = id;
        }

        protected override void OnAdded() {}
        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}
    }

    interface IIdProvider<T>
    {
        Id<T> Id { get; }
    }
}
