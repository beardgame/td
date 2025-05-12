using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class DieWhenAllHitPointsDepleted : Component, IListener<ComponentAdded>, IListener<ComponentRemoved>
{
    private readonly List<HitPointsPool> hitPointsPools = [];

    protected override void OnAdded()
    {
        hitPointsPools.AddRange(Owner.GetComponents<HitPointsPool>());
        Events.Subscribe<ComponentAdded>(this);
        Events.Subscribe<ComponentRemoved>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<ComponentAdded>(this);
        Events.Unsubscribe<ComponentRemoved>(this);
    }

    public void HandleEvent(ComponentAdded @event)
    {
        if (@event.Component is HitPointsPool pool)
        {
            hitPointsPools.Add(pool);
        }
    }

    public void HandleEvent(ComponentRemoved @event)
    {
        if (@event.Component is HitPointsPool pool)
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
