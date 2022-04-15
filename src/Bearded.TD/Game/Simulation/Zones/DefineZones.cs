using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Zones;

static class DefineZones
{
    public static ISerializableCommand<GameInstance> Command(
        GameInstance game, ImmutableArray<Zone> zones, ImmutableArray<(Zone, Zone)> connections)
        => new Implementation(game, zones, connections);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameInstance game;
        private readonly ImmutableArray<Zone> zones;
        private readonly ImmutableArray<(Zone, Zone)> connections;

        public Implementation(
            GameInstance game, ImmutableArray<Zone> zones, ImmutableArray<(Zone, Zone)> connections)
        {
            this.game = game;
            this.zones = zones;
            this.connections = connections;
        }

        public void Execute()
        {
            foreach (var zone in zones)
            {
                game.State.ZoneLayer.AddZone(zone);
            }
            foreach (var (from, to) in connections)
            {
                game.State.ZoneLayer.ConnectZones(from, to);
            }
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(zones, connections);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Zone?[] zones = Array.Empty<Zone>();
        private (Id<Zone>, Id<Zone>)[] connections = Array.Empty<(Id<Zone>, Id<Zone>)>();

        public Serializer(ImmutableArray<Zone> zones, ImmutableArray<(Zone, Zone)> connections)
        {
            this.zones = zones.ToArray();
            this.connections = connections.SelectBoth(z => z.Id).ToArray();
        }

        // ReSharper disable once UnusedMember.Local
        public Serializer() { }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            var zonesById = zones.ToImmutableDictionary(zone => zone.Id);
            var resolvedConnections = connections.SelectBoth(id => zonesById[id]).ToImmutableArray();
            return new Implementation(game, zones.ToImmutableArray(), resolvedConnections);
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.SerializeArrayCount(ref zones);
            for (var i = 0; i < zones.Length; i++)
            {
                serializeZone(stream, ref zones[i]);
            }
            stream.SerializeArrayCount(ref connections);
            for (var i = 0; i < connections.Length; i++)
            {
                serializeConnection(stream, ref connections[i]);
            }
        }

        private void serializeZone(INetBufferStream stream, ref Zone? zone)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            var id = Id<Zone>.Invalid;
            var tiles = ImmutableArray<Tile>.Empty;
            var visibilityTiles = ImmutableArray<Tile>.Empty;
            zone?.Deconstruct(out id, out tiles, out visibilityTiles);
            stream.Serialize(ref id);
            serializeTiles(stream, ref tiles);
            serializeTiles(stream, ref visibilityTiles);
            zone = new Zone(id, tiles, visibilityTiles);
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

        private void serializeConnection(INetBufferStream stream, ref (Id<Zone>, Id<Zone>) connection)
        {
            stream.Serialize(ref connection.Item1);
            stream.Serialize(ref connection.Item2);
        }
    }
}
