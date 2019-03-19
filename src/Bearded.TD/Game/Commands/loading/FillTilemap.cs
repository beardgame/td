using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Game.Commands
{
    static class FillTilemap
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, Tilemap<TileType> types, Tilemap<TileDrawInfo> drawInfos)
            => new Implementation(game, types.Select(t => types[t]).ToList(), drawInfos.Select(t => drawInfos[t]).ToList());

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly IList<TileType> types;
            private readonly IList<TileDrawInfo> drawInfos;

            public Implementation(GameInstance game, IList<TileType> types, IList<TileDrawInfo> drawInfos)
            {
                if (types.Count != drawInfos.Count)
                    throw new ArgumentException();

                this.game = game;
                this.types = types;
                this.drawInfos = drawInfos;
            }

            public void Execute()
            {
                game.MustBeLoading();

                var geometry = game.State.GeometryLayer;
                var tilemapForEnumeration = new Tilemap<Void>(game.State.Level.Radius);

                foreach (var (tile, i) in tilemapForEnumeration.Select((t, i) => (t, i)))
                {
                    geometry.SetTileType(tile, types[i], drawInfos[i]);
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(types, drawInfos);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private TileType[] types;
            private Unit[] drawHeights;
            private float[] drawSizeFactors;

            public Serializer(IList<TileType> types, IList<TileDrawInfo> drawInfos)
            {
                this.types = types.ToArray();
                drawHeights = drawInfos.Select(i => i.Height).ToArray();
                drawSizeFactors = drawInfos.Select(i => i.HexScale).ToArray();
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, types, Enumerable.Range(0, types.Length)
                    .Select(i => new TileDrawInfo(drawHeights[i], drawSizeFactors[i])).ToList()
                    );

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref types);
                drawHeights = drawHeights ?? new Unit[types.Length];
                drawSizeFactors = drawSizeFactors ?? new float[types.Length];
                var typeBytes = (byte[]) (object) types;
                foreach (var i in Enumerable.Range(0, types.Length))
                {
                    stream.Serialize(ref typeBytes[i]);
                    stream.Serialize(ref drawHeights[i]);
                    stream.Serialize(ref drawSizeFactors[i]);
                }
            }
        }
    }
}
