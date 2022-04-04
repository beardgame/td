using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed partial class AllowManualControl : Component, IManualControlReport
{
    private sealed record Control(Action Cancel, CrossHair CrossHair, Overdrive Overdrive);

    private Control? activeControl;

    private IFactionProvider? factionProvider;
    private IBuildingStateProvider? buildingState;
    public ReportType Type => ReportType.ManualControl;

    protected override void OnAdded()
    {
        ReportAggregator.Register(Events, this);
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, provider => buildingState = provider);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (activeControl == null)
            return;

        if (buildingState is { State.IsFunctional: false })
        {
            activeControl.Cancel();
        }
    }

    public bool CanBeControlledBy(Faction faction)
    {
        if (buildingState is { State.IsFunctional: false })
            return false;

        return factionProvider != null && factionProvider.Faction.OwnedBuildingsCanBeManuallyControlledBy(faction);
    }

    public Position2 SubjectPosition => Owner.Position.XY();
    public Unit SubjectRange { get; private set; }

    public void StartControl(IManualTarget2 target, Action cancelControl)
    {
        DebugAssert.State.Satisfies(activeControl == null);

        activeControl = new Control(cancelControl, new CrossHair(target), new Overdrive());

        Owner.AddComponent(activeControl.Overdrive);
        Owner.AddComponent(activeControl.CrossHair);

        SubjectRange = 3.U();

        foreach (var turret in Owner.GetComponents<ITurret>())
        {
            turret.OverrideTargeting(activeControl.CrossHair);
            if (turret.Weapon.TryGetSingleComponent<IWeaponRange>(out var range))
                SubjectRange = Math.Max(SubjectRange.NumericValue, range.Range.NumericValue).U();
        }
    }

    public void EndControl()
    {
        DebugAssert.State.Satisfies(activeControl != null);

        Owner.RemoveComponent(activeControl!.Overdrive);
        Owner.RemoveComponent(activeControl.CrossHair);

        activeControl = null;

        foreach (var turret in Owner.GetComponents<ITurret>())
        {
            turret.StopTargetOverride();
        }
    }
}
