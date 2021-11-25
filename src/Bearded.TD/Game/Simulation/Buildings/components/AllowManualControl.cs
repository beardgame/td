using System;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed partial class AllowManualControl<T> : Component<T>, IManualControlReport
        where T : IComponentOwner<T>, IGameObject, IPositionable
    {
        private CrossHair? crossHair;
        public ReportType Type => ReportType.ManualControl;

        protected override void OnAdded()
        {
            ReportAggregator.Register(Events, this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
        }

        public Position2 SubjectPosition => Owner.Position.XY();
        public Unit SubjectRange { get; private set; }

        public void StartControl(IManualTarget2 target)
        {
            DebugAssert.State.Satisfies(crossHair == null);

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

            Owner.RemoveComponent(crossHair!);
            crossHair = null;

            foreach (var turret in (Owner as IComponentOwner).GetComponents<ITurret>())
            {
                turret.StopTargetOverride();
            }
        }
    }
}
