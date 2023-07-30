using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
#if !DEBUG
using Bearded.TD.Game.Commands;
#endif
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.World;

static class FillTilemap
{
    public static ISerializableCommand<GameInstance> Command(
        GameInstance game,
        Tilemap<TileGeometry> tileGeometries,
        Tilemap<IBiome> biomes,
        Tilemap<TileDrawInfo> drawInfos) =>
        new Implementation(
            game,
            tileGeometries.Select(t => tileGeometries[t]).ToImmutableArray(),
            biomes.Select(t => biomes[t]).ToImmutableArray(),
            drawInfos.Select(t => drawInfos[t]).ToImmutableArray());

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameInstance game;
        private readonly IReadOnlyList<TileGeometry> tileGeometries;
        private readonly IReadOnlyList<IBiome> biomes;
        private readonly IReadOnlyList<TileDrawInfo> drawInfos;

        public Implementation(
            GameInstance game,
            IReadOnlyList<TileGeometry> tileGeometries,
            IReadOnlyList<IBiome> biomes,
            IReadOnlyList<TileDrawInfo> drawInfos)
        {
            if (tileGeometries.Count != biomes.Count || tileGeometries.Count != drawInfos.Count)
            {
                throw new ArgumentException("Tile geometries, biomes, and draw infos did not all have the same size.");
            }

            this.game = game;
            this.tileGeometries = tileGeometries;
            this.biomes = biomes;
            this.drawInfos = drawInfos;
        }

        public void Execute()
        {
#if !DEBUG
                game.MustBeLoading();
#endif

            var geometry = game.State.GeometryLayer;
            var biomeLayer = game.State.BiomeLayer;
            var tilemapForEnumeration = Tilemap.EnumerateTilemapWith(game.State.Level.Radius);

            foreach (var (tile, i) in tilemapForEnumeration.Select((t, i) => (t, i)))
            {
                geometry.SetTileGeometry(tile, tileGeometries[i], drawInfos[i]);
                biomeLayer.SetBiome(tile, biomes[i]);
            }
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(tileGeometries, biomes, drawInfos);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private byte[] types = Array.Empty<byte>();
        private double[]? hardnesses;
        private Unit[]? heights;
        private ModAwareId[]? biomes;
        private Unit[]? drawHeights;
        private float[]? drawSizeFactors;

        public Serializer(
            IReadOnlyList<TileGeometry> tileGeometries,
            IReadOnlyList<IBiome> biomes,
            IReadOnlyList<TileDrawInfo> drawInfos)
        {
            types = tileGeometries.Select(t => (byte) t.Type).ToArray();
            hardnesses = tileGeometries.Select(t => t.Hardness).ToArray();
            heights = tileGeometries.Select(t => t.FloorHeight).ToArray();
            this.biomes = biomes.Select(b => b.Id).ToArray();
            drawHeights = drawInfos.Select(i => i.Height).ToArray();
            drawSizeFactors = drawInfos.Select(i => i.HexScale).ToArray();
        }

        [UsedImplicitly]
        public Serializer() {}

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game,
                Enumerable.Range(0, types.Length)
                    .Select(i => new TileGeometry((TileType) types[i], hardnesses![i], heights![i]))
                    .ToImmutableArray(),
                Enumerable.Range(0, types.Length)
                    .Select(i => game.Blueprints.Biomes[biomes![i]])
                    .ToImmutableArray(),
                Enumerable.Range(0, types.Length)
                    .Select(i => new TileDrawInfo(drawHeights![i], drawSizeFactors![i]))
                    .ToImmutableArray()
            );

        public void Serialize(INetBufferStream stream)
        {
            stream.SerializeArrayCount(ref types);
            hardnesses ??= new double[types.Length];
            heights ??= new Unit[types.Length];
            biomes ??= new ModAwareId[types.Length];
            drawHeights ??= new Unit[types.Length];
            drawSizeFactors ??= new float[types.Length];
            foreach (var i in Enumerable.Range(0, types.Length))
            {
                stream.Serialize(ref types[i]);
                stream.Serialize(ref hardnesses[i]);
                stream.Serialize(ref heights[i]);
                stream.Serialize(ref biomes[i]);
                stream.Serialize(ref drawHeights[i]);
                stream.Serialize(ref drawSizeFactors[i]);
            }
        }
    }
}
