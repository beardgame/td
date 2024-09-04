using System;
using System.Reactive.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.UI;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Rules.Resources;

[GameRule("grantResourcesOnWaveComplete")]
sealed class GrantResourcesOnWaveComplete(GrantResourcesOnWaveComplete.RuleParameters parameters)
    : GameRule<GrantResourcesOnWaveComplete.RuleParameters>(parameters)
{
    [UsedImplicitly]
    public sealed record RuleParameters(
        ExternalId<Faction> Faction,
        ResourceType Type,
        double Amount,
        double AmountPerWaveInChapter,
        double AmountPerChapter,
        double AmountForLastWaveInChapter
    );

    public override void Execute(GameRuleContext context)
    {
        _ = context.Events.Observe<WaveEnded>().Select(calculateResourceAmount)
            .Subscribe(a => parameters.Type.Switch(a, grant, grant));

        double calculateResourceAmount(WaveEnded e)
        {
            var chapter = e.Wave.Script.ChapterNumber - 1;
            var wave = e.Wave.Script.WaveNumber - 1;
            var isFinalWave = e.Wave.Script.IsFinalWave;

            return parameters.Amount +
                wave * parameters.AmountPerWaveInChapter +
                chapter * parameters.AmountPerChapter +
                (isFinalWave ? parameters.AmountForLastWaveInChapter : 0);
        }

        void grant<T>(Resource<T> amount)
            where T : IResourceType
        {
            var faction = context.Factions.Find(Parameters.Faction);

            if (faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
            {
                resources.ProvideResources(amount);
            }
            else
            {
                context.Logger.Debug?.Log(
                    $"Tried providing resources at end of the wave to {faction.ExternalId}, " +
                    "but it doesn't have resources.");
            }
        }
    }
}
