using Bearded.TD.Game.Commands.Loading;
using Bearded.TD.Game.GameState.Buildings;
using Bearded.TD.Game.GameState.World;
using Newtonsoft.Json;

namespace Bearded.TD.Game.GameState.Rules.Buildings
{
    [GameRule("placeBuilding")]
    sealed class PlaceBuildingRule : GameRule<PlaceBuildingRule.Parameters>
    {
        public PlaceBuildingRule(Parameters parameters) : base(parameters) {}

        protected override void Execute(GameState owner, Parameters parameters)
        {
            owner.Meta.Dispatcher.RunOnlyOnServer(() => PlopBuilding.Command(
                owner,
                owner.RootFaction,
                owner.Meta.Ids.GetNext<Building>(),
                parameters.Building,
                parameters.Footprint));
        }

        public readonly struct Parameters
        {
            public IBuildingBlueprint Building { get; }
            public PositionedFootprint Footprint { get; }

            [JsonConstructor]
            public Parameters(IBuildingBlueprint building, PositionedFootprint footprint)
            {
                Building = building;
                Footprint = footprint;
            }
        }
    }
}
