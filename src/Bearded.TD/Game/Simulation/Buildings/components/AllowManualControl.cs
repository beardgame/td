using System;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed partial class AllowManualControl<T> : Component<T>, IManualControlReport
    where T : IComponentOwner<T>, IGameObject, IPositionable
{
    private CrossHair? crossHair;
    private Overdrive<T>? overdrive;
    private IFactionProvider? factionProvider;
    public ReportType Type => ReportType.ManualControl;

    protected override void OnAdded()
    {
        ReportAggregator.Register(Events, this);
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public bool CanBeControlledBy(Faction faction)
    {
        return factionProvider != null && factionProvider.Faction.SharesBehaviorWith<FactionResources>(faction);
    }

    public Position2 SubjectPosition => Owner.Position.XY();
    public Unit SubjectRange { get; private set; }

    public void StartControl(IManualTarget2 target)
    {
        DebugAssert.State.Satisfies(crossHair == null);
        DebugAssert.State.Satisfies(overdrive == null);

        Owner.AddComponent(overdrive = new Overdrive<T>());
        crossHair = new CrossHair(target);
        Owner.AddComponent(crossHair);

        SubjectRange = 3.U();

        foreach (var turret in (Owner as IComponentOwner).GetComponents<ITurret>())
        {
            turret.OverrideTargeting(crossHair);
            if (turret.Weapon.TryGetSingleComponent<IWeaponRange>(out var range))
                SubjectRange = Math.Max(SubjectRange.NumericValue, range.Range.NumericValue).U();
        }
    }

    public void EndControl()
    {
        DebugAssert.State.Satisfies(crossHair != null);
        DebugAssert.State.Satisfies(overdrive != null);

        Owner.RemoveComponent(overdrive!);
        Owner.RemoveComponent(crossHair!);
        overdrive = null;
        crossHair = null;

        foreach (var turret in (Owner as IComponentOwner).GetComponents<ITurret>())
        {
            turret.StopTargetOverride();
        }

    }
}
