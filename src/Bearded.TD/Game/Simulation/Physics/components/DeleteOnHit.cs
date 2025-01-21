using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Physics.DeleteOnHit;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("deleteOnHit")]
sealed class DeleteOnHit(IParameters parameters) : Component<IParameters>(parameters),
    IListener<CollideWithObject>, IListener<CollideWithLevel>
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
            Events.Subscribe<CollideWithObject>(this);

        if (!Parameters.ExcludeLevel)
            Events.Subscribe<CollideWithLevel>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public override void OnRemoved()
    {
        if (!Parameters.ExcludeObjects)
            Events.Unsubscribe<CollideWithObject>(this);

        if (!Parameters.ExcludeLevel)
            Events.Unsubscribe<CollideWithLevel>(this);
    }

    public void HandleEvent(CollideWithObject e)
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

    public void HandleEvent(CollideWithLevel e)
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
