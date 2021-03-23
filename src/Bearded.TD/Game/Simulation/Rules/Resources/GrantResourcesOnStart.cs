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
    sealed class GrantResourcesOnStart : GameRule<GrantResourcesOnStart.Parameters>
    {
        public GrantResourcesOnStart(Parameters parameters) : base(parameters) { }

        protected override void Execute(GameState game, Parameters parameters)
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

        public readonly struct Parameters
        {
            public ResourceAmount Amount { get; }

            [JsonConstructor]
            public Parameters(ResourceAmount amount)
            {
                Amount = amount;
            }
        }
    }
}
