using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Drones;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("fuelTank")]
sealed class FuelTank : Component<FuelTank.IParameters>, IListener<ShotProjectile>, IListener<WaveEnded>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.FuelCapacity)]
        public int FuelCapacity { get; }
    }

    private ImmutableArray<KnownWeapon> knownWeapons;
    private int fuelUsed;
    private WeaponDisabledReason? disabledReason;
    private DroneFulfillment? drone;

    public FuelTank(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

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
                return new KnownWeapon(weapon, weapon.GetComponents<IWeaponState>().Single(), spy);
            })
            .ToImmutableArray();
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        foreach (var knownWeapon in knownWeapons)
        {
            knownWeapon.Weapon.RemoveComponent(knownWeapon.Spy);
        }
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(ShotProjectile @event)
    {
        fuelUsed++;
        if (fuelUsed >= Parameters.FuelCapacity && disabledReason == null)
        {
            handleTankEmpty();
        }
    }

    public void HandleEvent(WaveEnded @event)
    {
        drone?.Cancel();
        refillTank();
    }

    private void handleTankEmpty()
    {
        disabledReason = new WeaponDisabledReason();
        foreach (var knownWeapon in knownWeapons)
        {
            knownWeapon.State.Disable(disabledReason);
        }
        requestRefuel();
    }

    private void requestRefuel()
    {
        var request = new DroneRequest(Level.GetTile(Owner.Position), refillTank);
        var requestEvent = new RequestDrone(request, null);
        Owner.Game.Meta.Events.Preview(ref requestEvent);
        if (requestEvent.FulfillmentPreview is { } preview)
        {
            drone = preview.Spawner.Fulfill(request, preview);
        }
    }

    private void refillTank()
    {
        disabledReason?.Resolve();
        disabledReason = null;
        drone = null;
    }

    private readonly record struct KnownWeapon(GameObject Weapon, IWeaponState State, ListenerComponent Spy);

    private sealed class ListenerComponent : Component, IListener<ShotProjectile>
    {
        private readonly FuelTank @delegate;

        public ListenerComponent(FuelTank @delegate)
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

        public override void Update(TimeSpan elapsedTime) {}
    }
}
