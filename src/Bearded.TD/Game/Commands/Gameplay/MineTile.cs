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
            => new Implementation(game, Id<MiningTaskPlaceholder>.Invalid, faction, tile);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly Id<MiningTaskPlaceholder> id;
            private readonly Faction faction;
            private readonly Tile tile;

            public Implementation(GameInstance game, Id<MiningTaskPlaceholder> id, Faction faction, Tile tile)
            {
                this.game = game;
                this.faction = faction;
                this.tile = tile;
                this.id = id;
            }

            public override bool CheckPreconditions()
            {
                return game.State.Level.IsValid(tile)
                       && game.State.GeometryLayer[tile].Type == TileType.Wall;
            }

            public override ISerializableCommand<GameInstance> ToCommand()
                => new Implementation(game, game.Meta.Ids.GetNext<MiningTaskPlaceholder>(), faction, tile);

            public override void Execute()
            {
                var placeholder = new MiningTaskPlaceholder(id, faction, tile);
                game.State.Add(placeholder);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(id, tile, faction);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<MiningTaskPlaceholder> id;
            private int tileX;
            private int tileY;
            private Id<Faction> faction;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Id<MiningTaskPlaceholder> id, Tile tile, Faction faction)
            {
                this.id = id;
                tileX = tile.X;
                tileY = tile.Y;
                this.faction = faction.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(game, id, game.State.FactionFor(faction), new Tile(tileX, tileY));
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref tileX);
                stream.Serialize(ref tileY);
                stream.Serialize(ref faction);
            }
        }
    }
}
