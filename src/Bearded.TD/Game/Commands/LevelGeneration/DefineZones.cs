using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands.LevelGeneration
{
    static class DefineZones
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, ImmutableArray<ZoneDefinition> zones)
            => new Implementation(game, zones);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly ImmutableArray<ZoneDefinition> zones;

            public Implementation(GameInstance game, ImmutableArray<ZoneDefinition> zones)
            {
                this.game = game;
                this.zones = zones;
            }

            public void Execute()
            {
                foreach (var zoneDefinition in zones)
                {
                    game.State.ZoneLayer.AddZone(zoneDefinition);
                }
            }

            ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(zones);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private ZoneDefinition?[] zones = Array.Empty<ZoneDefinition>();

            public Serializer(ImmutableArray<ZoneDefinition> zones)
            {
                this.zones = zones.ToArray();
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, zones.ToImmutableArray());

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref zones);
                for (var i = 0; i < zones.Length; i++)
                {
                    serializeZone(stream, ref zones[i]);
                }
            }

            private void serializeZone(INetBufferStream stream, ref ZoneDefinition? zone)
            {
                // ReSharper disable once InlineOutVariableDeclaration
                var id = Id<Zone>.Invalid;
                var tiles = ImmutableArray<Tile>.Empty;
                zone?.Deconstruct(out id, out tiles);
                stream.Serialize(ref id);
                serializeTiles(stream, ref tiles);
                zone = new ZoneDefinition(id, tiles);
            }

            private void serializeTiles(INetBufferStream stream, ref ImmutableArray<Tile> tiles)
            {
                var tilesArray = tiles.ToArray();
                stream.SerializeArrayCount(ref tilesArray);
                for (var i = 0; i < tilesArray.Length; i++)
                {
                    var (x, y) = tilesArray[i];
                    stream.Serialize(ref x);
                    stream.Serialize(ref y);
                    tilesArray[i] = new Tile(x, y);
                }

                tiles = tilesArray.ToImmutableArray();
            }
        }
    }
}
