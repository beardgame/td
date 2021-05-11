using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Newtonsoft.Json;

namespace Bearded.TD.Game.Simulation.Rules.Resources
{
    [GameRule("grantResourcesOnStart")]
    sealed class GrantResourcesOnStart : GameRule<GrantResourcesOnStart.RuleParameters>
    {
        public GrantResourcesOnStart(RuleParameters parameters) : base(parameters) { }

        protected override void Execute(GameState game, RuleParameters parameters)
        {
            game.Meta.Events.Subscribe(new Listener(game.Factions, parameters.Amount));
        }

        private sealed class Listener : IListener<GameStarted>
        {
            private readonly ReadOnlyCollection<Faction> factions;
            private readonly ResourceAmount amount;

            public Listener(ReadOnlyCollection<Faction> factions, ResourceAmount amount)
            {
                this.factions = factions;
                this.amount = amount;
            }

            public void HandleEvent(GameStarted @event)
            {
                foreach (var faction in factions.Where(f => f.HasResources))
                {
                    faction.Resources!.ProvideResources(amount);
                }
            }
        }

        public readonly struct RuleParameters
        {
            public ResourceAmount Amount { get; }

            [JsonConstructor]
            public RuleParameters(ResourceAmount amount)
            {
                Amount = amount;
            }
        }
    }
}
