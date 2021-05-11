using Bearded.TD.Game.Commands.Loading;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.World;
using Newtonsoft.Json;

namespace Bearded.TD.Game.Simulation.Rules.Buildings
{
    [GameRule("placeBuilding")]
    sealed class PlaceBuildingRule : GameRule<PlaceBuildingRule.RuleParameters>
    {
        public PlaceBuildingRule(RuleParameters parameters) : base(parameters) {}

        public override void Initialize(GameRuleContext context)
        {
            context.Dispatcher.RunOnlyOnServer(() => PlopBuilding.Command(
                context.GameState,
                context.RootFaction,
                context.Ids.GetNext<Building>(),
                Parameters.Building,
                Parameters.Footprint));
        }

        public readonly struct RuleParameters
        {
            public IBuildingBlueprint Building { get; }
            public PositionedFootprint Footprint { get; }

            [JsonConstructor]
            public RuleParameters(IBuildingBlueprint building, PositionedFootprint footprint)
            {
                Building = building;
                Footprint = footprint;
            }
        }
    }
}
