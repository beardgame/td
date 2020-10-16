﻿using System.IO;
using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Rendering;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.ShaderManagement;
using amulware.Graphics.Shapes;
using amulware.Graphics.Textures;
using amulware.Graphics.Vertices;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Utilities.Collections;
using OpenToolkit.Graphics.OpenGL;

namespace Bearded.TD.Rendering
{
    class SurfaceManager
    {
        private static readonly string workingDir = Directory.GetCurrentDirectory() + "/";

        public ShaderManager Shaders { get; } = new ShaderManager();

        public Matrix4Uniform ViewMatrix { get; } = new Matrix4Uniform("view");
        public Matrix4Uniform ViewMatrixLevel { get; } = new Matrix4Uniform("view");
        public Matrix4Uniform ProjectionMatrix { get; } = new Matrix4Uniform("projection");
        public FloatUniform FarPlaneDistance { get; } = new FloatUniform("farPlaneDistance");

        public Vector3Uniform FarPlaneBaseCorner { get; } = new Vector3Uniform("farPlaneBaseCorner");
        public Vector3Uniform FarPlaneUnitX { get; } = new Vector3Uniform("farPlaneUnitX");
        public Vector3Uniform FarPlaneUnitY { get; } = new Vector3Uniform("farPlaneUnitY");
        public Vector3Uniform CameraPosition { get; } = new Vector3Uniform("cameraPosition");

        public FloatUniform Time { get; } = new FloatUniform("time");

        public TextureUniform DepthBuffer { get; set; }

        public ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> Primitives { get; }
        public IRenderer PrimitivesRenderer { get; }
        public IndexedSurface<PrimitiveVertexData> ConsoleBackground { get; }
        public IndexedSurface<UVColorVertexData> ConsoleFontSurface { get; }
        public IndexedSurface<UVColorVertexData> UIFontSurface { get; }
        public Font ConsoleFont { get; }
        public Font UIFont { get; }

        public IndexedSurface<PointLightVertex> PointLights { get; }
        public IndexedSurface<SpotlightVertex> Spotlights { get; }

        public SurfaceManager()
        {
            var shaderPath = asset("shaders/");

#if DEBUG
            shaderPath = AdjustPathToReloadable(shaderPath);
#endif

            Shaders.Add(
                ShaderFileLoader.CreateDefault(shaderPath).Load(".")
            );
            new[]
            {
                "geometry", "uvcolor",
                "deferred/gSprite",
                "deferred/debug",
                "deferred/debugDepth",
                "deferred/compose",
                "deferred/copy",
                "deferred/copyDepth",
                "deferred/pointlight",
                "deferred/spotlight"
            }.ForEach(name => Shaders.MakeShaderProgram(name));

            Primitives = new ExpandingIndexedTrianglesMeshBuilder<ColorVertexData>();
            PrimitivesRenderer = BatchedRenderer.From(
                    Primitives.ToRenderable(), ViewMatrix, ProjectionMatrix);
            Shaders.TryGetRendererShader("geometry", out var primitiveShader);
            primitiveShader!.UseOnRenderer(PrimitivesRenderer);

            ConsoleBackground = new IndexedSurface<PrimitiveVertexData>()
                .WithShader(Shaders["geometry"])
                .AndSettings(ViewMatrix, ProjectionMatrix);

            ConsoleFont = Font.FromJsonFile(font("inconsolata.json"));
            ConsoleFontSurface = new IndexedSurface<UVColorVertexData>()
                .WithShader(Shaders["uvcolor"])
                .AndSettings(
                    ViewMatrix, ProjectionMatrix,
                    new TextureUniform("diffuse", new Texture(font("inconsolata.png"), preMultiplyAlpha: true))
                );

            UIFont = Font.FromJsonFile(font("helveticaneue.json"));
            UIFontSurface = new IndexedSurface<UVColorVertexData>()
                    .WithShader(Shaders["uvcolor"])
                    .AndSettings(
                        ViewMatrix, ProjectionMatrix,
                        new TextureUniform("diffuse", new Texture(font("helveticaneue.png"), preMultiplyAlpha: true))
                    );

            PointLights = new IndexedSurface<PointLightVertex>()
                .WithShader(Shaders["deferred/pointlight"])
                .AndSettings(ViewMatrix, ProjectionMatrix,
                    FarPlaneBaseCorner, FarPlaneUnitX, FarPlaneUnitY, CameraPosition);

            Spotlights = new IndexedSurface<SpotlightVertex>()
                .WithShader(Shaders["deferred/spotlight"])
                .AndSettings(ViewMatrix, ProjectionMatrix,
                    FarPlaneBaseCorner, FarPlaneUnitX, FarPlaneUnitY, CameraPosition);
        }

        public static string AdjustPathToReloadable(string file)
        {
            // point at asset files in the actual repo instead of the binary folder for easy live editing

            // your\td\path
            // \bin\Bearded.TD\Debug\ -> \src\Bearded.TD\
            // assets\file.ext

            var newFile = file
                .Replace("\\", "/")
                .Replace("/bin/Bearded.TD/Debug/", "/src/Bearded.TD/");

            return newFile;
        }

        public void InjectDeferredBuffer(Texture normalBuffer, Texture depthBuffer)
        {
            var normalUniform = new TextureUniform("normalBuffer", TextureUnit.Texture0, normalBuffer);
            DepthBuffer = new TextureUniform("depthBuffer", TextureUnit.Texture1, depthBuffer);

            PointLights.AddSettings(normalUniform, DepthBuffer);
            Spotlights.AddSettings(normalUniform, DepthBuffer);
        }

        private static string asset(string path) => workingDir + "assets/" + path;
        private static string font(string path) => asset("font/" + path);

    }
}
