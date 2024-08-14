using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using Bearded.Graphics;
using Bearded.Graphics.ShaderManagement;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using LogicalMesh = SharpGLTF.Schema2.Mesh;
using Mesh = Bearded.TD.Content.Models.Mesh;

namespace Bearded.TD.Rendering.Loading;

sealed partial class GraphicsLoader : IGraphicsLoader
{
    readonly record struct Bitmaps(string Name, Dictionary<string, Image<Bgra32>> BitmapsBySampler);
    readonly record struct PositionedBitmaps(Bitmaps Bitmaps, int X, int Y);
    readonly record struct PackedBitmaps(IReadOnlyList<PositionedBitmaps> PositionedBitmaps, int Width, int Height);

    // TODO: use mod specific shader managers (tricky bit: hot reload)
    private readonly ShaderManager shaderManager;
    private readonly IActionQueue glActions;
    private readonly Logger logger;

    public GraphicsLoader(ShaderManager shaderManager, IActionQueue glActionQueue, Logger logger)
    {
        this.shaderManager = shaderManager;
        glActions = glActionQueue;
        this.logger = logger;
    }

    public IRendererShader CreateRendererShader(IList<ModShaderFile> shaders, string shaderProgramName)
    {
        var shadersToAdd = shaders.Where(s => !shaderManager.Contains(s.Type, s.FriendlyName)).ToList();

        if (shaderManager.TryGetRendererShader(shaderProgramName, out var shaderProgram))
        {
            if (shadersToAdd.Count == 0)
                return shaderProgram;

            throw new InvalidDataException($"Different shader program with name {shaderProgramName} already exists.");
        }

        return glActions.Run(glOperations).Result;

        IRendererShader glOperations()
        {
            shadersToAdd.Select(shaderFile).ForEach(shaderManager.Add);

            return shaderManager.RegisterRendererShader(b =>
            {
                foreach (var (type, _, name) in shaders)
                {
                    b.With(type, name);
                }
            }, shaderProgramName);
        }

        ShaderFile shaderFile(ModShaderFile data)
        {
            return new ShaderFile(data.Type, data.Filepath, data.FriendlyName);
        }
    }

    public IModelImplementation CreateModel(ModelRoot modelRoot)
    {
        var meshes = modelRoot.LogicalMeshes.Select(loadMesh).ToImmutableArray();
        return new Model(meshes);
    }

    private Mesh loadMesh(LogicalMesh mesh)
    {
        if (mesh.Primitives.Count != 1)
        {
            throw new InvalidDataException("Only meshes with a single primitive are supported");
        }

        var primitive = mesh.Primitives[0];

        var positions = primitive.GetVertexAccessor("POSITION").AsVector3Array();
        var normals = primitive.GetVertexAccessor("NORMAL").AsVector3Array();
        var uvs = primitive.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
        var vertices = interleaveVertices(positions, normals, uvs);
        var vertexBufferTask = glActions.Run(() => createVertexBuffer(vertices));

        var indicesAsUInt = primitive.IndexAccessor.AsIndicesArray();
        // Instead of converting to unsigned shorts, we can always do CopyTo(uint[]) and upload that to avoid any
        // copying, but it doubles the amount of data we upload to the GPU. Choices, choices...
        var indicesAsUShort = indicesAsUInt.Select(i => (ushort) i).ToArray();
        var indexBufferTask = glActions.Run(() => createIndexBuffer(indicesAsUShort));

        var vertexBuffer = vertexBufferTask.Result;
        var indexBuffer = indexBufferTask.Result;

        return new Mesh(vertexBuffer, indexBuffer);
    }

    private static NormalUVVertex[] interleaveVertices(
        IList<Vector3> positions, IList<Vector3> normals, IList<Vector2> uvs)
    {
        var array = new NormalUVVertex[positions.Count];
        for (var i = 0; i < positions.Count; i++)
        {
            array[i] = new NormalUVVertex(vec3(positions[i]), vec3(normals[i]), vec2(uvs[i]));
        }
        return array;
    }

    private static OpenTK.Mathematics.Vector2 vec2(Vector2 v) => new(v.X, v.Y);
    private static OpenTK.Mathematics.Vector3 vec3(Vector3 v) => new(v.X, v.Y, v.Z);

    private static Buffer<NormalUVVertex> createVertexBuffer(NormalUVVertex[] vertexArray)
    {
        var buffer = new Buffer<NormalUVVertex>();
        using var target = buffer.Bind();
        target.Upload(vertexArray);
        // TODO: return AsVertices
        return buffer;
    }

    private static Buffer<ushort> createIndexBuffer(ushort[] indexArray)
    {
        var buffer = new Buffer<ushort>();
        using var target = buffer.Bind();
        target.Upload(indexArray);
        // TODO: return AsIndices
        return buffer;
    }
}
