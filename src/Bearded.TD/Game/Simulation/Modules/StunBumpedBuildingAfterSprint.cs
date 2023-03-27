using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("stunBumpedBuildingAfterSprint")]
sealed class StunBumpedBuildingAfterSprint
    : Component<StunBumpedBuildingAfterSprint.IParameters>, IListener<StoppedSprinting>, IListener<HitLevel>
{
    private Instant stunUntil;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan TimeAfterSprint { get; }
        TimeSpan StunDuration { get; }
    }

    public StunBumpedBuildingAfterSprint(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe<StoppedSprinting>(this);
        Events.Subscribe<HitLevel>(this);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(StoppedSprinting _)
    {
        stunUntil = Owner.Game.Time + Parameters.TimeAfterSprint;
    }

    public void HandleEvent(HitLevel e)
    {
        if (stunUntil < Owner.Game.Time)
            return;

        if (Owner.Game.BuildingLayer.TryGetMaterializedBuilding(e.Tile, out var building))
        {
            Owner.Sync(StunObject.Command, building, Parameters.StunDuration);
        }
    }
}

