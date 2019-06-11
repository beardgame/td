using Bearded.TD.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Workers;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class MineTile
    {
        public static IRequest<GameInstance> Request(GameInstance game, Faction faction, Tile tile)
            => new Implementation(game, faction, tile, Id<IWorkerTask>.Invalid);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly Faction faction;
            private readonly Tile tile;
            private readonly Id<IWorkerTask> taskId;

            public Implementation(GameInstance game, Faction faction, Tile tile, Id<IWorkerTask> taskId)
            {
                this.game = game;
                this.faction = faction;
                this.tile = tile;
                this.taskId = taskId;
            }

            public override bool CheckPreconditions()
            {
                return game.State.Level.IsValid(tile)
                       && game.State.GeometryLayer[tile].Type == TileType.Wall;
            }

            public override ISerializableCommand<GameInstance> ToCommand()
                => new Implementation(game, faction, tile, game.Meta.Ids.GetNext<IWorkerTask>());

            public override void Execute()
            {
                var placeholder = new MiningTaskPlaceholder(faction, tile, taskId);
                game.State.Add(placeholder);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(tile, faction, taskId);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private int tileX;
            private int tileY;
            private Id<Faction> faction;
            private Id<IWorkerTask> taskId;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Tile tile, Faction faction, Id<IWorkerTask> taskId)
            {
                this.taskId = taskId;
                tileX = tile.X;
                tileY = tile.Y;
                this.faction = faction.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(game, game.State.FactionFor(faction), new Tile(tileX, tileY), taskId);
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref tileX);
                stream.Serialize(ref tileY);
                stream.Serialize(ref faction);
                stream.Serialize(ref taskId);
            }
        }
    }
}
