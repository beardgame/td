using System;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drones;

[Component("droneSpawner")]
sealed class DroneSpawner : Component<DroneSpawner.IParameters>, IDroneSpawner, IPreviewListener<RequestDrone>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        public ComponentOwnerBlueprint Drone { get; }
    }

    private IFactionProvider? factionProvider;
    private bool activated;

    public Tile Location => Level.GetTile(Owner.Position);

    public DroneSpawner(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, p => factionProvider = p);
    }

    public override void Activate()
    {
        base.Activate();
        Owner.Game.Meta.Events.Subscribe(this);
        activated = true;
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        if (activated)
        {
            Owner.Game.Meta.Events.Unsubscribe(this);
        }
    }

    public override void Update(TimeSpan elapsedTime) { }

    public DroneFulfillment Fulfill(DroneRequest request, DroneFulfillmentPreview preview)
    {
        var faction = factionProvider?.Faction ??
            throw new InvalidOperationException("Cannot fulfill drone requests without faction");
        var drone = DroneFactory.CreateDrone(
            Parameters.Drone, Owner.Position, faction, request, preview.Path.Path, out var droneComp);
        Owner.Game.Add(drone);
        return new DroneFulfillment(droneComp);
    }

    public void PreviewEvent(ref RequestDrone @event)
    {
        if (factionProvider?.Faction is { } faction &&
            faction.SharesBehaviorWith<ShareDrones>(@event.Faction) &&
            this.TryFulfillRequest(@event.Request, out var preview))
        {
            @event = @event.OfferAlternative(preview);
        }
    }
}
