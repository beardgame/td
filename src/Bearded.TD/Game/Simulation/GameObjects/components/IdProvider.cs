using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class IdProvider : Component, IIdProvider
{
    public Id<ComponentGameObject> Id { get; }

    public IdProvider(Id<ComponentGameObject> id)
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

interface IIdProvider
{
    Id<ComponentGameObject> Id { get; }
}
