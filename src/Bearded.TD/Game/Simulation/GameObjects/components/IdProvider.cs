using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class IdProvider<T> : Component<T>, IIdProvider<T>
    where T : IGameObject
{
    public Id<T> Id { get; }

    public IdProvider(Id<T> id)
    {
        Id = id;
    }

    protected override void OnAdded()
    {
        Owner.Game.IdAs(Id, Owner);
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        Owner.Game.DeleteId(Id);
    }

    public override void Update(TimeSpan elapsedTime) {}
}

interface IIdProvider<T>
{
    Id<T> Id { get; }
}