using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Rules.Resources
{
    [GameRule("grantResourcesOnStart")]
    sealed class GrantResourcesOnStart : GameRule<GrantResourcesOnStart.RuleParameters>
    {
        public GrantResourcesOnStart(RuleParameters parameters) : base(parameters) { }

        public override void Execute(GameRuleContext context)
        {
            context.Events.Subscribe(new Listener(context.Factions.Find(Parameters.Faction), Parameters.Amount));
        }

        private sealed class Listener : IListener<GameStarted>
        {
            private readonly Faction faction;
            private readonly ResourceAmount amount;

            public Listener(Faction faction, ResourceAmount amount)
            {
                this.faction = faction;
                this.amount = amount;
            }

            public void HandleEvent(GameStarted @event)
            {
                faction.Resources!.ProvideResources(amount);
            }
        }

        public sealed record RuleParameters(ExternalId<Faction> Faction, ResourceAmount Amount);
    }
}
