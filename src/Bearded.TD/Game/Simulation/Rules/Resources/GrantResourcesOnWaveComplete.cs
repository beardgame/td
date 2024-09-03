using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.IO;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Rules.Resources;

[GameRule("grantResourcesOnWaveComplete")]
sealed class GrantResourcesOnWaveComplete : GameRule<GrantResourcesOnWaveComplete.RuleParameters>
{
    public GrantResourcesOnWaveComplete(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(
            new Listener(context.Logger, context.Factions.Find(Parameters.Faction), Parameters.Amount));
    }

    private sealed class Listener : IListener<WaveEnded>
    {
        private readonly Logger logger;
        private readonly Faction faction;
        private readonly Resource<Scrap> amount;

        public Listener(Logger logger, Faction faction, Resource<Scrap> amount)
        {
            this.logger = logger;
            this.faction = faction;
            this.amount = amount;
        }

        public void HandleEvent(WaveEnded @event)
        {
            if (@event.TargetFaction == faction &&
                faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
            {
                resources.ProvideResources(amount);
            }
            else
            {
                logger.Debug?.Log(
                    $"Tried providing resources at end of the wave to {faction.ExternalId}, " +
                    "but it doesn't have resources.");
            }
        }
    }

    [UsedImplicitly]
    public sealed record RuleParameters(ExternalId<Faction> Faction, Resource<Scrap> Amount);
}
