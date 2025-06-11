using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Physics.DeleteOnHit;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("deleteOnHit")]
sealed class DeleteOnHit(IParameters parameters) : Component<IParameters>(parameters),
    IListener<ObjectHit>, IListener<CollidedWithLevel>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        bool ExcludeObjects { get; }
        bool ExcludeLevel { get; }

        int ObjectCount { get; }
        int TilesCount { get; }
    }

    private HashSet<GameObject>? objectsHit;
    private HashSet<Tile>? tilesHit;

    protected override void OnAdded()
    {
        if (!Parameters.ExcludeObjects)
            Events.Subscribe<ObjectHit>(this);

        if (!Parameters.ExcludeLevel)
            Events.Subscribe<CollidedWithLevel>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    protected override void OnRemovedInternal()
    {
        if (!Parameters.ExcludeObjects)
            Events.Unsubscribe<ObjectHit>(this);

        if (!Parameters.ExcludeLevel)
            Events.Unsubscribe<CollidedWithLevel>(this);
    }


    public void HandleEvent(ObjectHit e)
    {
        if (Parameters.ObjectCount > 1)
        {
            objectsHit ??= [];
            objectsHit.Add(e.Object);

            if (objectsHit.Count < Parameters.ObjectCount)
                return;
        }

        Owner.Delete();
    }

    public void HandleEvent(CollidedWithLevel e)
    {
        if (Parameters.TilesCount > 1)
        {
            tilesHit ??= [];
            tilesHit.Add(e.Tile);

            if (tilesHit.Count < Parameters.TilesCount)
                return;
        }

        Owner.Delete();
    }
}
