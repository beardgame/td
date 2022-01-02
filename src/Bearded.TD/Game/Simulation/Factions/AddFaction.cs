using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Game.Simulation.Factions;

[GameRule("addFaction")]
sealed class AddFaction : GameRule<AddFaction.RuleParameters>
{
    public AddFaction(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        var parent = Parameters.Parent == null ? null : context.Factions.Find(Parameters.Parent.Value);
        context.GameState.AddFaction(
            Faction.FromBlueprint(context.Ids.GetNext<Faction>(), parent, Parameters.Blueprint, context.Events));
    }

    public sealed record RuleParameters(IFactionBlueprint Blueprint, ExternalId<Faction>? Parent);
}