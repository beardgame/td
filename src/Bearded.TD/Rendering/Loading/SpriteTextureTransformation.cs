using System;
using System.Linq;
using Bearded.Graphics.Textures;

namespace Bearded.TD.Rendering.Loading;

static class SpriteTextureTransformations
{
    public static ITextureTransformation DoNothing { get; } = new SpriteTextureTransformation(_ => { });

    public static ITextureTransformation Test { get; } = new SpriteTextureTransformation(
        data =>
        {
            var green = data.Green;
            foreach (var y in Enumerable.Range(0, data.Height))
                foreach (var x in Enumerable.Range(0, data.Width))
                {
                    green[x, y] = 255;
                }
        }
    );
}

sealed class SpriteTextureTransformation : ITextureTransformation
{
    public sealed class Data
    {
        public readonly struct ColorChannel
        {
            private readonly Data data;
            private readonly int offset;

            public ColorChannel(Data data, int offset)
            {
                this.data = data;
                this.offset = offset;
            }

            public byte this[int x, int y]
            {
                get
                {
                    var i = data.flat(x, y) + offset;
                    return data.bytes[i];
                }
                set
                {
                    var i = data.flat(x, y) + offset;
                    data.bytes[i] = value;
                }
            }
        }

        private readonly byte[] bytes;

        public int Width { get; }
        public int Height { get; }

        public ColorChannel Red => new(this, 2);
        public ColorChannel Green => new(this, 1);
        public ColorChannel Blue => new(this, 0);
        public ColorChannel Alpha => new(this, 3);

        public Data(byte[] bytes, int width, int height)
        {
            Width = width;
            Height = height;
            this.bytes = bytes;
        }

        public Graphics.Color this[int x, int y]
        {
            get
            {
                var i = flat(x, y);
                return new Graphics.Color(bytes[i + 2], bytes[i + 1], bytes[i], bytes[i + 3]);
            }
            set
            {
                var i = flat(x, y);
                bytes[i + 2] = value.R;
                bytes[i + 1] = value.G;
                bytes[i] = value.B;
                bytes[i + 3] = value.A;
            }
        }

        private int flat(int x, int y) => 4 * (y * Width + x);
    }

    private readonly Action<Data> transform;

    public SpriteTextureTransformation(Action<Data> transform)
    {
        this.transform = transform;
    }

    public void Transform(ref byte[] data, ref int width, ref int height)
    {
        transform(new Data(data, width, height));
    }
}
