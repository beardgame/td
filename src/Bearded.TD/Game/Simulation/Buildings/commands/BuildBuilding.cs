using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class BuildBuilding
{
    public static IRequest<Player, GameInstance> Request(
        GameInstance game,
        Faction faction,
        IComponentOwnerBlueprint blueprint,
        PositionedFootprint footprint) =>
        new Implementation(
            game, faction, Id<ComponentGameObject>.Invalid, blueprint, footprint, Id<IWorkerTask>.Invalid);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameInstance game;
        private readonly Faction faction;
        private readonly Id<ComponentGameObject> id;
        private readonly IComponentOwnerBlueprint blueprint;
        private readonly PositionedFootprint footprint;
        private readonly Id<IWorkerTask> taskId;

        public Implementation(
            GameInstance game,
            Faction faction,
            Id<ComponentGameObject> id,
            IComponentOwnerBlueprint blueprint,
            PositionedFootprint footprint,
            Id<IWorkerTask> taskId)
        {
            this.game = game;
            this.faction = faction;
            this.id = id;
            this.blueprint = blueprint;
            this.footprint = footprint;
            this.taskId = taskId;
        }

        public override bool CheckPreconditions(Player actor)
        {
            if (!faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var factionTechnology))
            {
                return false;
            }

            return factionTechnology.IsBuildingUnlocked(blueprint)
                && faction.SharesBehaviorWith<FactionResources>(actor.Faction)
                && componentPreconditionsAreMet();
        }

        private bool componentPreconditionsAreMet()
        {
            var preconditionParameters = new IBuildBuildingPrecondition.Parameters(
                game.State, footprint);

            return blueprint.GetBuildBuildingPreconditions()
                .All(c => c.CanBuild(preconditionParameters).IsValid);
        }

        public override ISerializableCommand<GameInstance> ToCommand() => new Implementation(
            game,
            faction,
            game.Meta.Ids.GetNext<ComponentGameObject>(),
            blueprint,
            footprint,
            game.Meta.Ids.GetNext<IWorkerTask>());

        public override void Execute()
        {
            var building = new BuildingFactory(game.State).Create(id, blueprint, faction, footprint);
            building.AddComponent(new BuildingConstructionWork<ComponentGameObject>(taskId));
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() =>
            new Serializer(faction, id, blueprint, footprint, taskId);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Faction> faction;
        private ModAwareId blueprint;
        private ModAwareId footprint;
        private int footprintIndex;
        private Id<ComponentGameObject> id;
        private int footprintX;
        private int footprintY;
        private Id<IWorkerTask> taskId;

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(
            Faction faction,
            Id<ComponentGameObject> id,
            IBlueprint blueprint,
            PositionedFootprint footprint,
            Id<IWorkerTask> taskId)
        {
            this.id = id;
            this.faction = faction.Id;
            this.blueprint = blueprint.Id;
            this.footprint = footprint.Footprint.Id;
            footprintIndex = footprint.FootprintIndex;
            footprintX = footprint.RootTile.X;
            footprintY = footprint.RootTile.Y;
            this.taskId = taskId;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(
                game,
                game.State.Factions.Resolve(faction),
                id,
                game.Blueprints.ComponentOwners[blueprint],
                new PositionedFootprint(
                    game.Blueprints.Footprints[footprint], footprintIndex,
                    new Tile(footprintX, footprintY)),
                taskId);
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
            stream.Serialize(ref id);
            stream.Serialize(ref blueprint);
            stream.Serialize(ref footprint);
            stream.Serialize(ref footprintIndex);
            stream.Serialize(ref footprintX);
            stream.Serialize(ref footprintY);
            stream.Serialize(ref taskId);
        }
    }
}
