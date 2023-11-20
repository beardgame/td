using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Drones;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Elements;

interface IFuelSystem
{
    void HandleTankEmpty();
}

[Component("fuelSystem")]
sealed class FuelSystem : Component,
    IFuelSystem,
    IListener<ComponentAdded>,
    IListener<ComponentRemoved>,
    IListener<ShotProjectile>,
    IListener<WaveEnded>
{
    private bool activated;
    private ImmutableArray<KnownWeapon> knownWeapons = ImmutableArray<KnownWeapon>.Empty;
    private readonly List<IFuelTank> knownTanks = new();
    private IBreakageHandler? breakageHandler;
    private IFactionProvider? factionProvider;
    private IBreakageReceipt? breakage;
    private IFuelTank? activeTank;
    private DroneFulfillment? drone;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IBreakageHandler>(Owner, Events, h => breakageHandler = h);
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, p => factionProvider = p);
        Events.Subscribe<ComponentAdded>(this);
        Events.Subscribe<ComponentRemoved>(this);
        Events.Subscribe<ShotProjectile>(this);
    }

    public override void Activate()
    {
        base.Activate();
        Owner.Game.Meta.Events.Subscribe<WaveEnded>(this);
        knownWeapons = Owner.GetComponents<ITurret>()
            .Select(t =>
            {
                var weapon = t.Weapon;
                var spy = new ListenerComponent(this);
                weapon.AddComponent(spy);
                return new KnownWeapon(weapon, spy);
            })
            .ToImmutableArray();
        knownTanks.AddRange(Owner.GetComponents<IFuelTank>());
        replaceTank();
        activated = true;
    }

    public override void OnRemoved()
    {
        base.OnRemoved();

        foreach (var knownWeapon in knownWeapons)
        {
            knownWeapon.Weapon.RemoveComponent(knownWeapon.Spy);
        }

        if (activated)
        {
            Owner.Game.Meta.Events.Unsubscribe<WaveEnded>(this);
        }
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(ComponentAdded @event)
    {
        if (!activated || @event.Component is not IFuelTank addedTank)
        {
            return;
        }

        knownTanks.Add(addedTank);
        if (activeTank is null)
        {
            replaceTank();
        }
    }

    public void HandleEvent(ComponentRemoved @event)
    {
        if (!activated || @event.Component is not IFuelTank removedTank)
        {
            return;
        }

        if (knownTanks.Remove(removedTank))
        {
            replaceTank();
        }
    }

    public void HandleEvent(ShotProjectile @event)
    {
        State.Satisfies(activeTank != null);
        if (activeTank is null) return;

        activeTank.Consume();
        if (activeTank.IsEmpty)
        {
            Owner.Sync(HandleEmptyFuelTank.Command);
        }
    }

    public void HandleEvent(WaveEnded @event)
    {
        drone?.Cancel();
        drone = null;
        breakage?.Repair();
        breakage = null;
        foreach (var t in knownTanks)
        {
            t.Refill();
        }
        replaceTank();
    }

    public void HandleTankEmpty()
    {
        if (activeTank is not null)
        {
            requestRefuel(activeTank);
        }
        replaceTank();
    }

    private void requestRefuel(IFuelTank tank)
    {
        if (factionProvider == null)
        {
            Owner.Game.Meta.Logger.Warning?.Log($"{Owner} attempted to request fuel but does not belong to faction.");
            return;
        }

        var request = new DroneRequest(Level.GetTile(Owner.Position), () => refillTank(tank));
        var requestEvent = new RequestDrone(factionProvider.Faction, request, null);
        Owner.Game.Meta.Events.Preview(ref requestEvent);
        if (requestEvent.FulfillmentPreview is { } preview)
        {
            drone = preview.Spawner.Fulfill(request, preview);
        }
    }

    private void refillTank(IFuelTank tank)
    {
        tank.Refill();
        if (activeTank is null)
        {
            replaceTank();
        }
    }

    private void replaceTank()
    {
        activeTank = knownTanks.FirstOrDefault(t => !t.IsEmpty);
        if (activeTank is null && breakage is null)
        {
            disableTower();
        }
        else if (activeTank is not null && breakage is not null)
        {
            enableTower();
        }
    }

    private void enableTower()
    {
        breakage?.Repair();
        breakage = null;
    }

    private void disableTower()
    {
        breakage = breakageHandler?.BreakObject();
    }

    private readonly record struct KnownWeapon(GameObject Weapon, ListenerComponent Spy);

    private sealed class ListenerComponent : Component, IListener<ShotProjectile>
    {
        private readonly FuelSystem @delegate;

        public ListenerComponent(FuelSystem @delegate)
        {
            this.@delegate = @delegate;
        }

        protected override void OnAdded()
        {
            Events.Subscribe(this);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            Events.Unsubscribe(this);
        }

        public void HandleEvent(ShotProjectile @event)
        {
            @delegate.HandleEvent(@event);
        }

        public override void Update(TimeSpan elapsedTime) { }
    }
}
