using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.IO;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Rules.Resources;

[GameRule("grantResourcesOnStart")]
sealed class GrantResourcesOnStart : GameRule<GrantResourcesOnStart.RuleParameters>
{
    public GrantResourcesOnStart(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(
            new Listener(context.Logger, context.Factions.Find(Parameters.Faction), Parameters.Amount));
    }

    private sealed class Listener : IListener<GameStarted>
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

        public void HandleEvent(GameStarted @event)
        {
            if (faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
            {
                resources.ProvideResources(amount);
            }
            else
            {
                logger.Debug?.Log(
                    $"Tried providing resources at start of the game to {faction.ExternalId}, " +
                    "but it doesn't have resources.");
            }
        }
    }

    [UsedImplicitly]
    public sealed record RuleParameters(ExternalId<Faction> Faction, Resource<Scrap> Amount);
}
