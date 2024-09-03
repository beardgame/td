using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using JetBrains.Annotations;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Buildings;

static class BuildBuilding
{
    public static IRequest<Player, GameInstance> Request(
        GameInstance game,
        Faction faction,
        IGameObjectBlueprint blueprint,
        PositionedFootprint footprint) =>
        new Implementation(game, faction, Id<GameObject>.Invalid, blueprint, footprint);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameInstance game;
        private readonly Faction faction;
        private readonly Id<GameObject> id;
        private readonly IGameObjectBlueprint blueprint;
        private readonly PositionedFootprint footprint;

        public Implementation(
            GameInstance game,
            Faction faction,
            Id<GameObject> id,
            IGameObjectBlueprint blueprint,
            PositionedFootprint footprint)
        {
            this.game = game;
            this.faction = faction;
            this.id = id;
            this.blueprint = blueprint;
            this.footprint = footprint;
        }

        public override bool CheckPreconditions(Player actor)
        {
            if (!faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var factionTechnology))
            {
                return false;
            }
            if (!faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var factionResources))
            {
                return false;
            }

            var buildingPreconditions = preconditionsResult();

            return factionTechnology.IsBuildingUnlocked(blueprint)
                && faction.SharesBehaviorWith<FactionResources>(actor.Faction)
                && buildingPreconditions.IsValid
                && factionResources.GetCurrent<Scrap>() >= buildingPreconditions.Cost;
        }

        public override ISerializableCommand<GameInstance> ToCommand() => new Implementation(
            game,
            faction,
            game.Meta.Ids.GetNext<GameObject>(),
            blueprint,
            footprint);

        public override void Execute()
        {
            var result = preconditionsResult();
            State.Satisfies(result.IsValid);

            var building = BuildingFactory.Create(id, blueprint, faction, footprint);
            game.State.Add(building);

            var constructionSyncer = building.GetComponents<IBuildingConstructionSyncer>().Single();
            constructionSyncer.SyncStartBuild();
            constructionSyncer.SyncCompleteBuild();

            faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources);
            resources!.ConsumeResources(result.Cost);

            if (building.GetComponents<IBreakageHandler>().SingleOrDefault() is { } breakageHandler)
            {
                var receipt = breakageHandler.BreakObject();
                building.Delay(receipt.Repair, 5.S());
            }
        }

        private IBuildBuildingPrecondition.Result preconditionsResult()
        {
            var preconditionParameters = new IBuildBuildingPrecondition.Parameters(game.State, footprint);
            var result = blueprint.GetBuildBuildingPreconditions()
                .Select(c => c.CanBuild(preconditionParameters))
                .Aggregate(IBuildBuildingPrecondition.Result.Valid, IBuildBuildingPrecondition.Result.And);
            return result;
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() =>
            new Serializer(faction, id, blueprint, footprint);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Faction> faction;
        private ModAwareId blueprint;
        private Id<GameObject> id;
        private ModAwareId footprint;
        private int footprintX;
        private int footprintY;
        private Orientation orientation;

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(
            Faction faction,
            Id<GameObject> id,
            IBlueprint blueprint,
            PositionedFootprint footprint)
        {
            this.id = id;
            this.faction = faction.Id;
            this.blueprint = blueprint.Id;
            this.footprint = footprint.Footprint?.Id ?? ModAwareId.Invalid;
            footprintX = footprint.RootTile.X;
            footprintY = footprint.RootTile.Y;
            orientation = footprint.Orientation;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(
                game,
                game.State.Factions.Resolve(faction),
                id,
                game.Blueprints.GameObjects[blueprint],
                new PositionedFootprint(
                    footprint.IsValid ? game.Blueprints.Footprints[footprint] : PositionedFootprint.Invalid,
                    new Tile(footprintX, footprintY),
                    orientation));
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
            stream.Serialize(ref id);
            stream.Serialize(ref blueprint);
            stream.Serialize(ref footprint);
            stream.Serialize(ref footprintX);
            stream.Serialize(ref footprintY);
            stream.Serialize(ref orientation);
        }
    }
}
