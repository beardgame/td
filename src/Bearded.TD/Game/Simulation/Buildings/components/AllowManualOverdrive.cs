using System;
using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using Overrider = Bearded.TD.Game.Simulation.Buildings.ExclusiveOverrider<Bearded.TD.Game.Simulation.Buildings.AllowManualOverdrive.ActiveOverdrive>;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("allowManualOverdrive")]
sealed class AllowManualOverdrive : Component<AllowManualOverdrive.IParameters>, Overrider.IOverrideImplementation, IManualOverdrive
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(6.0)]
        public TimeSpan ActiveDuration { get; }

        [Modifiable(6.0)]
        public TimeSpan BreakageDuration { get; }

        ImmutableArray<IUpgradeEffect> Effects { get; }
    }

    private Overrider overrider = null!;
    private IStatusReceipt? statusReceipt;
    private Instant? overrideStart;
    private ActiveBreakage? breakage;

    public sealed record ActiveOverdrive(Action Cancel, Overdrive Overdrive) : Overrider.Override(Cancel);

    private sealed record ActiveBreakage(IBreakageReceipt Receipt, Instant Start);

    public AllowManualOverdrive(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        overrider = Overrider.CreateSubscribed(Owner, Events, this);
    }

    public override void Activate()
    {
        if (!Owner.TryGetSingleComponent<IStatusTracker>(out var statusTracker))
        {
            return;
        }

        statusReceipt = statusTracker.AddStatus(
            new StatusSpec(StatusType.Neutral, new InteractionSpec(this)), noProgressAppearance(), null);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        overrider.Update();

        if (overrideStart is not null)
        {
            statusReceipt?.UpdateAppearance(
                progressAppearance((Owner.Game.Time - overrideStart.Value) / Parameters.ActiveDuration));

            if (Owner.Game.Time - overrideStart > Parameters.ActiveDuration)
            {
                overrider.EndOverride();
            }
        }

        if (breakage is not null)
        {
            statusReceipt?.UpdateAppearance(
                progressAppearance(1 - (Owner.Game.Time - breakage.Start) / Parameters.BreakageDuration));

            if (Owner.Game.Time - breakage.Start > Parameters.BreakageDuration)
            {
                breakage.Receipt.Repair();
                breakage = null;
                statusReceipt?.UpdateAppearance(noProgressAppearance());
            }
        }
    }

    protected override void OnRemovedInternal()
    {
        statusReceipt?.DeleteImmediately();
    }

    public bool CanBeEnabledBy(Faction faction) => overrider.CanBeOverriddenBy(faction);

    public void StartOverdrive(Action cancelOverdrive)
    {
        var control = new ActiveOverdrive(cancelOverdrive, new Overdrive(Upgrade.FromEffects(Parameters.Effects)));
        overrider.StartOverride(control);
    }

    public void EndOverdrive()
    {
        overrider.EndOverride();
    }

    public void OnOverrideStart(ActiveOverdrive control)
    {
        State.Satisfies(breakage is null);
        Owner.AddComponent(control.Overdrive);
        overrideStart = Owner.Game.Time;
        statusReceipt?.UpdateAppearance(progressAppearance(0));
        Events.Send(new OverdriveStarted());
    }

    public void OnOverrideEnd(ActiveOverdrive control)
    {
        Owner.RemoveComponent(control.Overdrive);
        overrideStart = null;
        if (Owner.TryGetSingleComponent<IBreakageHandler>(out var breakageHandler))
        {
            breakage = new ActiveBreakage(breakageHandler.BreakObject(), Owner.Game.Time);
            statusReceipt?.UpdateAppearance(progressAppearance(1));
        }
        else
        {
            statusReceipt?.UpdateAppearance(noProgressAppearance());
        }
        Events.Send(new OverdriveEnded());
    }

    private static StatusAppearance noProgressAppearance() => StatusAppearance.IconOnly(statusIcon);
    private static StatusAppearance progressAppearance(double progress) =>
        StatusAppearance.IconAndProgress(statusIcon, progress).Disabled();

    private static ModAwareSpriteId statusIcon => "orb-direction".ToStatusIconSpriteId();

    private sealed class InteractionSpec(AllowManualOverdrive subject) : IStatusInteractionSpec
    {
        public Keys? ShortcutKey => OpenTK.Windowing.GraphicsLibraryFramework.Keys.O;

        public void Interact(GameRequestDispatcher requestDispatcher, Player player)
        {
            if (!subject.CanBeEnabledBy(player.Faction)) return;
            requestDispatcher.Request(StartManualOverdrive.Request, subject.Owner, player.Faction);
        }
    }
}
