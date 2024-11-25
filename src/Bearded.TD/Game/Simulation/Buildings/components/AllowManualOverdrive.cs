using System;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("allowManualOverdrive")]
sealed class AllowManualOverdrive : AllowManualOverride<AllowManualOverdrive.ActiveOverdrive>, IManualOverdrive
{
    private IStatusReceipt? statusReceipt;
    private Instant? overrideStart;
    private ActiveBreakage? breakage;

    public sealed record ActiveOverdrive(Action Cancel, Overdrive Overdrive) : Override(Cancel);

    private sealed record ActiveBreakage(IBreakageReceipt Receipt, Instant Start);

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
        base.Update(elapsedTime);

        if (overrideStart is not null)
        {
            statusReceipt?.UpdateAppearance(
                progressAppearance((Owner.Game.Time - overrideStart.Value) / Constants.Game.Overdrive.UpgradeDuration));

            if (Owner.Game.Time - overrideStart > Constants.Game.Overdrive.UpgradeDuration)
            {
                EndOverride();
            }
        }

        if (breakage is not null)
        {
            statusReceipt?.UpdateAppearance(
                progressAppearance(1 - (Owner.Game.Time - breakage.Start) / Constants.Game.Overdrive.BreakageDuration));

            if (Owner.Game.Time - breakage.Start > Constants.Game.Overdrive.BreakageDuration)
            {
                breakage.Receipt.Repair();
                breakage = null;
                statusReceipt?.UpdateAppearance(noProgressAppearance());
            }
        }
    }

    public override void OnRemoved()
    {
        statusReceipt?.DeleteImmediately();
    }

    public bool CanBeEnabledBy(Faction faction) => CanBeOverriddenBy(faction);

    public void StartOverdrive(Action cancelOverdrive)
    {
        var control = new ActiveOverdrive(cancelOverdrive, new Overdrive());
        StartOverride(control);
    }

    public void EndOverdrive()
    {
        EndOverride();
    }

    protected override void OnOverrideStart(ActiveOverdrive control)
    {
        State.Satisfies(breakage is null);
        Owner.AddComponent(control.Overdrive);
        overrideStart = Owner.Game.Time;
        statusReceipt?.UpdateAppearance(progressAppearance(0));
    }

    protected override void OnOverrideEnd(ActiveOverdrive control)
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
    }

    private static StatusAppearance noProgressAppearance() => StatusAppearance.IconOnly(statusIcon);
    private static StatusAppearance progressAppearance(double progress) =>
        StatusAppearance.IconAndProgress(statusIcon, progress);

    private static ModAwareSpriteId statusIcon => "orb-direction".ToStatusIconSpriteId();

    private sealed class InteractionSpec(AllowManualOverdrive subject) : IStatusInteractionSpec
    {
        public void Interact(GameRequestDispatcher requestDispatcher, Player player)
        {
            if (!subject.CanBeEnabledBy(player.Faction)) return;
            requestDispatcher.Request(StartManualOverdrive.Request, subject.Owner, player.Faction);
        }
    }
}
