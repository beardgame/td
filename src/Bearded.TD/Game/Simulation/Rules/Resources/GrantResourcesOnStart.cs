using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Rules.Resources;

[GameRule("grantResourcesOnStart")]
sealed class GrantResourcesOnStart(GrantResourcesOnStart.RuleParameters parameters)
    : GameRule<GrantResourcesOnStart.RuleParameters>(parameters)
{
    [UsedImplicitly]
    public sealed record RuleParameters(
        ExternalId<Faction> Faction,
        ResourceType Type,
        double Amount
    );

    public override void Execute(GameRuleContext context)
    {
        _ = context.Events.Subscribe<GameStarted>(_ => parameters.Type.Switch(parameters.Amount, grant, grant));

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
                    $"Tried providing resources at start of the game to {faction.ExternalId}, " +
                    "but it doesn't have resources.");
            }
        }
    }
}
