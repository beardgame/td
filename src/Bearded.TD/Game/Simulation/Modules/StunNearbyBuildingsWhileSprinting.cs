using System;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("stunNearbyBuildingsWhileSprinting")]
sealed class StunNearbyBuildingsWhileSprinting
    : Component<StunNearbyBuildingsWhileSprinting.IParameters>, IListener<StartedSprinting>, IListener<StoppedSprinting>
{
    private readonly Random random = new();
    private bool sprinting;
    private Instant nextStun;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan StunDuration { get; }
        TimeSpan TimeBetweenStuns { get; }
        TimeSpan DelayAfterSprintStart { get; }
        Unit Range { get; }
    }

    public StunNearbyBuildingsWhileSprinting(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe<StartedSprinting>(this);
        Events.Subscribe<StoppedSprinting>(this);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (!sprinting)
            return;

        if (nextStun <= Owner.Game.Time)
            tryStun();
    }

    private void tryStun()
    {
        nextStun += Parameters.TimeBetweenStuns;

        var buildings = Owner.Game.BuildingLayer;
        var building = Level.TilesWithCenterInCircle(Owner.Position.XY(), Parameters.Range)
            .Select(t => { buildings.TryGetMaterializedBuilding(t, out var b); return b; })
            .NotNull()
            .RandomElementOrDefault(random);

        if (building == null)
            return;

        Owner.Sync(StunObject.Command, building, Parameters.StunDuration);
    }

    public void HandleEvent(StartedSprinting _)
    {
        sprinting = true;
        nextStun = Owner.Game.Time + Parameters.DelayAfterSprintStart;
    }

    public void HandleEvent(StoppedSprinting _)
    {
        sprinting = false;
    }
}

