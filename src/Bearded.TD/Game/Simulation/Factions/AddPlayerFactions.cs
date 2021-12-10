using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Factions;

[GameRule("addPlayerFactions")]
sealed class AddPlayerFactions : GameRule<AddPlayerFactions.RuleParameters>
{
    public AddPlayerFactions(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        var parent = Parameters.Parent == null ? null : context.Factions.Find(Parameters.Parent.Value);
        foreach (var (player, i) in context.Players.Indexed())
        {
            // TODO: the mod file should probably have an impact on what colours should be assigned to players
            var playerColor = Color.FromHSVA(i * MathConstants.TwoPi / 6, 1, 1f);
            var faction = Faction.FromBlueprint(
                context.Ids.GetNext<Faction>(),
                parent,
                new PatchedBlueprint(Parameters.Blueprint, player, playerColor),
                context.Events);
            context.GameState.AddFaction(faction);
            player.SetFaction(faction);
        }
    }

    public sealed record RuleParameters(IFactionBlueprint Blueprint, ExternalId<Faction>? Parent);

    private sealed class PatchedBlueprint : IFactionBlueprint
    {
        private readonly IFactionBlueprint inner;
        private readonly Player player;

        public ExternalId<Faction> Id => ExternalId<Faction>.Invalid;
        public string Name => player.Name;
        public Color? Color { get; }

        public PatchedBlueprint(IFactionBlueprint inner, Player player, Color color)
        {
            this.inner = inner;
            this.player = player;
            Color = color;
        }

        public IEnumerable<IFactionBehavior<Faction>> GetBehaviors() => inner.GetBehaviors();

        ModAwareId IBlueprint.Id => ModAwareId.Invalid;
    }
}