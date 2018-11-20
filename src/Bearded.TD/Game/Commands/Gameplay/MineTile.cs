using Bearded.TD.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Resources;
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
            => new Implementation(game, faction, tile);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly Faction faction;
            private readonly Tile tile;

            public Implementation(GameInstance game, Faction faction, Tile tile)
            {
                this.game = game;
                this.faction = faction;
                this.tile = tile;
            }

            public override bool CheckPreconditions()
            {
                return tile.IsValid && tile.Info.IsMineable;
            }

            public override void Execute()
            {
                var placeholder = new MiningTaskPlaceholder(faction, tile);
                game.State.Add(placeholder);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(tile, faction);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private int tileX;
            private int tileY;
            private Id<Faction> faction;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Tile tile, Faction faction)
            {
                tileX = tile.X;
                tileY = tile.Y;
                this.faction = faction.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(game, game.State.FactionFor(faction), new Tile(tileX, tileY));
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref tileX);
                stream.Serialize(ref tileY);
                stream.Serialize(ref faction);
            }
        }
    }
}
