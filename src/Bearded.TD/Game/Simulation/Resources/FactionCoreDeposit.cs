using System;
using System.Collections.Immutable;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.UI;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Resources;

[FactionBehavior("coreDeposit")]
sealed class FactionCoreDeposit : FactionBehavior<FactionCoreDeposit.BehaviorParameters>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public FactionCoreDeposit(BehaviorParameters parameters) : base(parameters) { }

    [UsedImplicitly]
    public sealed record BehaviorParameters(
        double Amount,
        double AmountPerWaveInChapter,
        double AmountPerChapter,
        double AmountForLastWaveInChapter,
        ImmutableArray<WithdrawEvent> WithdrawEvents);

    [UsedImplicitly]
    public sealed record WithdrawEvent(double WaveProgress, double CumulativePercentageWithdrawn);

    private CurrentWaveDeposit? currentWaveDeposit;

    public Resource<CoreEnergy> AvailableCoreInCurrentWave =>
        currentWaveDeposit?.AvailableCore ?? Resource<CoreEnergy>.Zero;

    protected override void Execute()
    {
        _ = Events.Observe<WaveStarted>().Subscribe(onWaveStart);
    }

    private void onWaveStart(WaveStarted waveStarted)
    {
        if (currentWaveDeposit is not null)
        {
            throw new InvalidOperationException(
                "A new wave was started while a core deposit still exists for a previous wave");
        }

        var energy = calculateResourceAmount(waveStarted.Wave).CoreEnergy();
        var deposit = new CurrentWaveDeposit(energy);
        currentWaveDeposit = deposit;

        var progress = waveStarted.Progress;

        if (!Parameters.WithdrawEvents.IsDefaultOrEmpty)
        {
            foreach (var e in Parameters.WithdrawEvents)
            {
                progress.AddScriptedEvent(
                    e.WaveProgress,
                    () => withdraw(deposit, e.CumulativePercentageWithdrawn));
            }
        }

        progress.AddScriptedEvent(1, () => onWaveFinished(deposit));
    }

    private void onWaveFinished(CurrentWaveDeposit deposit)
    {
        if (currentWaveDeposit != deposit)
        {
            throw new InvalidOperationException(
                "The tracked deposit on wave completion is not the same as the expected deposit");
        }

        withdraw(deposit, 1);
        currentWaveDeposit = null;
    }

    private void withdraw(CurrentWaveDeposit deposit, double percentage)
    {
        var withdrawn = deposit.WithdrawUpToPercentage(percentage);
        if (withdrawn == Resource<CoreEnergy>.Zero) return;
        if (!Owner.TryGetBehavior<FactionResources>(out var resources))
        {
            throw new InvalidOperationException(
                "Cannot convert core deposit in resources for faction without resources");
        }
        resources.ProvideResources(withdrawn);
    }

    private double calculateResourceAmount(Wave wave)
    {
        var chapterNo = wave.Script.ChapterNumber - 1;
        var waveNo = wave.Script.WaveNumber - 1;
        var isFinalWave = wave.Script.IsFinalWave;

        return Parameters.Amount +
            waveNo * Parameters.AmountPerWaveInChapter +
            chapterNo * Parameters.AmountPerChapter +
            (isFinalWave ? Parameters.AmountForLastWaveInChapter : 0);
    }

    private sealed class CurrentWaveDeposit(Resource<CoreEnergy> initialAmount)
    {
        private Resource<CoreEnergy> alreadyWithdrawn;

        public Resource<CoreEnergy> AvailableCore
        {
            get
            {
                var available = initialAmount - alreadyWithdrawn;
                return available <= Resource<CoreEnergy>.Zero ? Resource<CoreEnergy>.Zero : available;
            }
        }

        public Resource<CoreEnergy> WithdrawUpToPercentage(double percentage)
        {
            var totalToLiquidate = percentage.Clamped(0, 1) * initialAmount;
            if (alreadyWithdrawn >= totalToLiquidate)
            {
                return Resource<CoreEnergy>.Zero;
            }
            var liquidatedNow = totalToLiquidate - alreadyWithdrawn;
            alreadyWithdrawn = totalToLiquidate;
            return liquidatedNow;
        }

        // TODO: call this when EMP is triggered
        public Resource<CoreEnergy> WithdrawImmediately(double effectiveness)
        {
            if (alreadyWithdrawn >= initialAmount)
            {
                return Resource<CoreEnergy>.Zero;
            }
            var available = AvailableCore;
            alreadyWithdrawn = initialAmount;
            return effectiveness * available;
        }
    }
}
