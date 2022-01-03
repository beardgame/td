using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.World;

static class FillTilemap
{
    public static ISerializableCommand<GameInstance> Command(
        GameInstance game, Tilemap<TileGeometry> tileGeometries, Tilemap<TileDrawInfo> drawInfos) =>
        new Implementation(
            game,
            tileGeometries.Select(t => tileGeometries[t]).ToList(), drawInfos.Select(t => drawInfos[t]).ToList());

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameInstance game;
        private readonly IList<TileGeometry> tileGeometries;
        private readonly IList<TileDrawInfo> drawInfos;

        public Implementation(GameInstance game, IList<TileGeometry> tileGeometries, IList<TileDrawInfo> drawInfos)
        {
            if (tileGeometries.Count != drawInfos.Count)
                throw new ArgumentException();

            this.game = game;
            this.tileGeometries = tileGeometries;
            this.drawInfos = drawInfos;
        }

        public void Execute()
        {
#if !DEBUG
                game.MustBeLoading();
#endif

            var geometry = game.State.GeometryLayer;
            var tilemapForEnumeration = Tilemap.EnumerateTilemapWith(game.State.Level.Radius);

            foreach (var (tile, i) in tilemapForEnumeration.Select((t, i) => (t, i)))
            {
                geometry.SetTileGeometry(tile, tileGeometries[i], drawInfos[i]);
            }
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(tileGeometries, drawInfos);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private byte[] types = {};
        private double[]? hardnesses;
        private Unit[]? heights;
        private Unit[]? drawHeights;
        private float[]? drawSizeFactors;

        public Serializer(IList<TileGeometry> tileGeometries, IList<TileDrawInfo> drawInfos)
        {
            types = tileGeometries.Select(t => (byte) t.Type).ToArray();
            hardnesses = tileGeometries.Select(t => t.Hardness).ToArray();
            heights = tileGeometries.Select(t => t.FloorHeight).ToArray();
            drawHeights = drawInfos.Select(i => i.Height).ToArray();
            drawSizeFactors = drawInfos.Select(i => i.HexScale).ToArray();
        }

        [UsedImplicitly]
        public Serializer() {}

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game,
                Enumerable.Range(0, types.Length)
                    .Select(i => new TileGeometry((TileType) types[i], hardnesses![i], heights![i]))
                    .ToList(),
                Enumerable.Range(0, types.Length)
                    .Select(i => new TileDrawInfo(drawHeights![i], drawSizeFactors![i]))
                    .ToList()
            );

        public void Serialize(INetBufferStream stream)
        {
            stream.SerializeArrayCount(ref types);
            hardnesses ??= new double[types.Length];
            heights ??= new Unit[types.Length];
            drawHeights ??= new Unit[types.Length];
            drawSizeFactors ??= new float[types.Length];
            foreach (var i in Enumerable.Range(0, types.Length))
            {
                stream.Serialize(ref types[i]);
                stream.Serialize(ref hardnesses[i]);
                stream.Serialize(ref heights[i]);
                stream.Serialize(ref drawHeights[i]);
                stream.Serialize(ref drawSizeFactors[i]);
            }
        }
    }
}
