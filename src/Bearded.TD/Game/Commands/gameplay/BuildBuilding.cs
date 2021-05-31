using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class BuildBuilding
    {
        public static IRequest<Player, GameInstance> Request(
                GameInstance game, Faction faction, IBuildingBlueprint blueprint, PositionedFootprint footprint) =>
            new Implementation(
                game, faction, Id<BuildingPlaceholder>.Invalid, blueprint, footprint, Id<IWorkerTask>.Invalid);

        private sealed class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly Faction faction;
            private readonly Id<BuildingPlaceholder> id;
            private readonly IBuildingBlueprint blueprint;
            private readonly PositionedFootprint footprint;
            private readonly Id<IWorkerTask> taskId;

            public Implementation(
                GameInstance game,
                Faction faction,
                Id<BuildingPlaceholder> id,
                IBuildingBlueprint blueprint,
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

            public override bool CheckPreconditions(Player actor) =>
                blueprint.FootprintGroup == footprint.Footprint
                && game.State.BuildingPlacementLayer.IsFootprintValidForBuilding(footprint)
                && faction.Technology.IsBuildingUnlocked(blueprint)
                && faction.SharesWorkersWith(actor.Faction);

            public override ISerializableCommand<GameInstance> ToCommand() => new Implementation(
                game,
                faction,
                game.Meta.Ids.GetNext<BuildingPlaceholder>(),
                blueprint,
                footprint,
                game.Meta.Ids.GetNext<IWorkerTask>());

            public override void Execute()
            {
                var placeholder = new BuildingPlaceholder(id, blueprint, faction, footprint, taskId);
                game.State.Add(placeholder);
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
            private Id<BuildingPlaceholder> id;
            private int footprintX;
            private int footprintY;
            private Id<IWorkerTask> taskId;

            [UsedImplicitly]
            public Serializer() {}

            public Serializer(
                Faction faction,
                Id<BuildingPlaceholder> id,
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
                    game.Blueprints.Buildings[blueprint],
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
}
