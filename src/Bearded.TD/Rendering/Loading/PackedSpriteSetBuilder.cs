using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Rendering;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Threading;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Mathematics;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Shader = Bearded.TD.Content.Models.Shader;

namespace Bearded.TD.Rendering.Loading
{
    class PackedSpriteSetBuilder
    {
        private readonly int width;
        private readonly int height;
        private readonly Dictionary<string, byte[]> samplerData = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, Rectangle> nameToRectangle = new Dictionary<string, Rectangle>();

        public PackedSpriteSetBuilder(List<string> samplers, int width, int height)
        {
            this.width = width;
            this.height = height;
            foreach (var sampler in samplers)
            {
                samplerData[sampler] = new byte[width * height * 4];
            }
        }

        public void CopyBitmap(string sprite, string sampler, Bitmap bitmap, int x, int y)
        {
            nameToRectangle[sprite] = new Rectangle(x, y, bitmap.Width, bitmap.Height);

            var data = samplerData[sampler];

            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var scan0 = bitmapData.Scan0;
            var stride = bitmapData.Stride;

            for (var i = 0; i < bitmap.Height; i++)
            {
                var start = scan0 + stride * i;
                Marshal.Copy(
                    start,
                    data,
                    flatCoordinate(x, y + i),
                    stride);
            }

            bitmap.UnlockBits(bitmapData);
        }

        private int flatCoordinate(int x, int y)
            => 4 * (y * width + x);

        public PackedSpriteSet Build(IActionQueue glActions, Shader shader, bool pixelate)
        {
            var premultiply = TextureTransformation.Premultiply;

            samplerData.Values.ForEach(bytes =>
            {
                var (w, h) = (width, height);
                premultiply.Transform(ref bytes, ref w, ref h);
            });

            // TODO: mesh builder should not be a responsibility of the packed sprite set so we can use multiple mesh
            //     builders for different use cases using the same sprites.
            var (textureUniforms, meshBuilder) = glActions.RunAndReturn(() => createGlEntities(pixelate));
            var sprites = createSprites(meshBuilder);

            return new PackedSpriteSet(
                textureUniforms,
                meshBuilder,
                sprites,
                shader
            );
        }

        private (List<TextureUniform>, ExpandingIndexedTrianglesMeshBuilder<UVColorVertex>)
            createGlEntities(bool pixelate)
        {
            var textureUniforms = samplerData.Select(
                (kvp, i) =>
                {
                    var (name, data) = kvp;
                    var texture = TextureData.From(data, width, height).ToTexture(t =>
                    {
                        if (pixelate)
                        {
                            t.SetFilterMode(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
                            t.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
                        }
                        else
                        {
                            t.GenerateMipmap();
                        }
                    });

                    return new TextureUniform(name, TextureUnit.Texture0 + i, texture);
                }
            ).ToList();

            var meshBuilder = new ExpandingIndexedTrianglesMeshBuilder<UVColorVertex>();

            return (textureUniforms, meshBuilder);
        }

        private Dictionary<string, ISprite> createSprites(IIndexedTrianglesMeshBuilder<UVColorVertex, ushort> meshBuilder)
        {
            return nameToRectangle.ToDictionary(
                pair => pair.Key,
                pair => createSprite(meshBuilder, pair.Value)
            );
        }

        private ISprite createSprite(IIndexedTrianglesMeshBuilder<UVColorVertex, ushort> meshBuilder, Rectangle rectangle)
        {
            var uv = toUvRectangle(rectangle);

            return new Sprite(meshBuilder, uv, new Vector2(rectangle.Width, rectangle.Height) * Constants.Rendering.PixelSize);
        }

        private UVRectangle toUvRectangle(Rectangle rect)
        {
            return new UVRectangle(
                (float)rect.Left / width, (float)rect.Right / width,
                (float)rect.Top / height, (float)rect.Bottom / height);
        }
    }
}
