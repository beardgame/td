using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage;

sealed partial class DieWhenAllHitPointsDepleted : Component
{
    private readonly List<HitPointsPool> hitPointsPools = [];

    protected override void OnAdded()
    {
        hitPointsPools.AddRange(Owner.GetComponents<HitPointsPool>());
    }

    [Handler]
    private void onComponentAdded(ComponentAdded e)
    {
        if (e.Component is HitPointsPool pool)
        {
            hitPointsPools.Add(pool);
        }
    }

    [Handler]
    private void onComponentRemoved(ComponentRemoved e)
    {
        if (e.Component is HitPointsPool pool)
        {
            hitPointsPools.Remove(pool);
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (hitPointsPools.Count > 0 && hitPointsPools.All(pool => pool.CurrentHitPoints <= HitPoints.Zero))
        {
            Events.Send(new EnactDeath());
        }
    }
}
