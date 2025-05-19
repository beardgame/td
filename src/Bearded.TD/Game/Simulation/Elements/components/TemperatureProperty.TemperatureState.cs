using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.Elements;

namespace Bearded.TD.Game.Simulation.Elements;

sealed partial class TemperatureProperty
{
    private bool extremeStatusActive;
    private IBreakageReceipt? breakage;
    private IStatusReceipt? regularStatus;
    private IStatusReceipt? extremeStatus;

    private IUpgradeReceipt? slowDownEffect;

    private abstract class TemperatureState
    {
        public static TemperatureState Normal { get; } = new NormalState();
        public static TemperatureState Hot { get; } = new HotState();
        public static TemperatureState Cold { get; } = new ColdState();

        private sealed class NormalState : TemperatureState;

        private sealed class HotState() : EscalatingState
        (
            new EscalatingParameters(
                RegularIcon: "thermometer-hot".ToStatusIconSpriteId(),
                ExtremeIcon: "hot-surface".ToStatusIconSpriteId(),
                RegularThreshold: MaxNormalTemperature,
                ExtremeThreshold: MaxTemperature,
                RecoverThreshold: MaxNormalTemperature
            )
        )
        {
            protected override void StartExtremeStatus(TemperatureProperty property)
            {
                tryBreakOwner(property);
                property.Events.Send(new Overheated());
            }

            protected override void StopExtremeStatus(TemperatureProperty property)
            {
                tryRepairOwner(property);
                property.Events.Send(new StopOverheated());
            }
        }

        private sealed class ColdState() : EscalatingState
        (
            new EscalatingParameters(
                RegularIcon: "thermometer-cold".ToStatusIconSpriteId(),
                ExtremeIcon: "snowflake-2".ToStatusIconSpriteId(),
                RegularThreshold: MinNormalTemperature,
                ExtremeThreshold: MinTemperature,
                RecoverThreshold: FrozenRecoveryTemperature
            )
        )
        {
            protected override void UpdateRegularStatus(TemperatureProperty property, float progress)
            {
                base.UpdateRegularStatus(property, progress);

                property.slowDownEffect?.Rollback();

                var effectStrength = progress.Clamped(0, 1);
                var speedFactor = Interpolate.Lerp(1, 0, effectStrength).Squared();
                var fireRateFactor = Interpolate.Lerp(1, 0.3f, effectStrength);

                var effect = Upgrade.FromEffects(
                    new ModifyParameter(
                        AttributeType.TurnSpeed,
                        Modification.MultiplyWith(speedFactor),
                        UpgradePrerequisites.Empty,
                        false
                    ),
                    new ModifyParameter(
                        AttributeType.FireRate,
                        Modification.MultiplyWith(fireRateFactor),
                        UpgradePrerequisites.Empty,
                        false
                    ));

                property.Owner.TryApplyUpgrade(effect, out property.slowDownEffect);
            }

            public override void Stop(TemperatureProperty property)
            {
                base.Stop(property);

                property.slowDownEffect?.Rollback();
                property.slowDownEffect = null;
            }

            protected override void StartExtremeStatus(TemperatureProperty property)
            {
                tryBreakOwner(property);
                property.Events.Send(new Frozen());
            }

            protected override void StopExtremeStatus(TemperatureProperty property)
            {
                tryRepairOwner(property);
                property.Events.Send(new StopFrozen());
            }
        }

        private static void tryBreakOwner(TemperatureProperty property)
        {
            if (property.Owner.TryGetSingleComponent<IBreakageHandler>(out var breakageHandler))
                property.breakage = breakageHandler.BreakObject();
        }

        private static void tryRepairOwner(TemperatureProperty property)
        {
            property.breakage?.Repair();
            property.breakage = null;
        }

        private readonly record struct EscalatingParameters(
            ModAwareSpriteId RegularIcon,
            ModAwareSpriteId ExtremeIcon,
            Temperature RegularThreshold,
            Temperature ExtremeThreshold,
            Temperature RecoverThreshold
        );

        private abstract class EscalatingState(EscalatingParameters parameters) : TemperatureState
        {
            protected EscalatingParameters Parameters => parameters;

            public override void Update(TemperatureProperty property)
            {
                var progress = Progress(property.Value, parameters.RegularThreshold, parameters.ExtremeThreshold);
                UpdateRegularStatus(property, progress);
                updateExtremeStatus(property, progress);
            }

            protected float Progress(Temperature value, Temperature zeroValue, Temperature oneValue)
            {
                return (value - zeroValue) / (oneValue - zeroValue);
            }

            public override void Stop(TemperatureProperty property)
            {
                property.regularStatus?.DeleteImmediately();
                property.regularStatus = null;
                property.extremeStatus?.DeleteImmediately();
                property.extremeStatus = null;
            }

            protected virtual void UpdateRegularStatus(TemperatureProperty property, float progress)
            {
                var appearance = StatusAppearance.IconAndProgress(parameters.RegularIcon, progress);

                if (property.regularStatus is { } status)
                {
                    status.UpdateAppearance(appearance);
                    return;
                }

                property.regularStatus = property.statusDisplay?.AddStatus(
                    new StatusSpec(StatusType.Neutral, null), appearance,
                    null
                );
            }

            private void updateExtremeStatus(TemperatureProperty property, float progress)
            {
                var shouldStartExtremeStatus =
                    !property.extremeStatusActive &&
                    progress >= 1;

                var shouldStopExtremeStatus =
                    property.extremeStatusActive &&
                    !shouldStartExtremeStatus &&
                    Progress(property.Value, Parameters.ExtremeThreshold, Parameters.RecoverThreshold) > 1;

                if (shouldStartExtremeStatus)
                {
                    property.extremeStatusActive = true;
                    property.extremeStatus = property.statusDisplay?.AddStatus(
                        new StatusSpec(StatusType.Negative, null), StatusAppearance.IconOnly(parameters.ExtremeIcon),
                        null
                    );

                    StartExtremeStatus(property);
                }
                else if (shouldStopExtremeStatus)
                {
                    property.extremeStatusActive = false;
                    property.extremeStatus?.DeleteImmediately();
                    property.extremeStatus = null;

                    StopExtremeStatus(property);
                }
            }

            protected abstract void StartExtremeStatus(TemperatureProperty property);
            protected abstract void StopExtremeStatus(TemperatureProperty property);
        }

        public virtual void Start(TemperatureProperty property) { }
        public virtual void Update(TemperatureProperty property) { }
        public virtual void Stop(TemperatureProperty property) { }
    }
}
