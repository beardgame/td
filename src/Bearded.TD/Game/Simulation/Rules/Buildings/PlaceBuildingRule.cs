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

        protected override void Execute(GameState owner, RuleParameters parameters)
        {
            owner.Meta.Dispatcher.RunOnlyOnServer(() => PlopBuilding.Command(
                owner,
                owner.RootFaction,
                owner.Meta.Ids.GetNext<Building>(),
                parameters.Building,
                parameters.Footprint));
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
