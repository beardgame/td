using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Bearded.Graphics;
using Bearded.Graphics.ImageSharp;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.ShaderManagement;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;
using OpenTK.Graphics.OpenGL;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using LogicalMaterial = SharpGLTF.Schema2.Material;
using LogicalMesh = SharpGLTF.Schema2.Mesh;
using Texture = Bearded.Graphics.Textures.Texture;

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

    public IMeshesImplementation CreateMeshes(ModelRoot modelRoot)
    {
        var materials = modelRoot.LogicalMaterials.ToImmutableDictionary(m => m, loadMaterial);
        var meshes = modelRoot.LogicalMeshes.ToImmutableDictionary(m => m.Name, m => loadMesh(m, materials));
        return new UploadedMeshes(meshes);
    }

    private MeshMaterial loadMaterial(LogicalMaterial material)
    {
        var tasks = MeshTextureChannel.All
            .Select(c => (c.FindChannel(material)?.Texture?.PrimaryImage?.Content, c.UniformName))
            .Where(c => c.Content.HasValue)
            .Select((c, i) =>
                glActions.Run(() => createTextureUniform(c.UniformName, c.Content!.Value, TextureUnit.Texture0 + i)))
            .ToList();

        return new MeshMaterial(tasks.Select(t => t.Result));
    }

    private readonly record struct MeshTextureChannel(
        string Name, string? AlternativeName, string UniformName, bool Required)
    {
        public static MeshTextureChannel Diffuse { get; } = new("BaseColor", "Diffuse", "diffuse", true);
        public static MeshTextureChannel Normal { get; } = new("Normal", null, "normal", false);
        public static MeshTextureChannel OcclusionMetallicRoughness { get; } = new("MetallicRoughness", null, "occlusionMetallicRoughness", false);

        public static IReadOnlyCollection<MeshTextureChannel> All => [Diffuse, Normal, OcclusionMetallicRoughness];

        public MaterialChannel? FindChannel(LogicalMaterial material)
        {
            var channel = material.FindChannel(Name);
            if (channel == null && AlternativeName != null)
            {
                channel = material.FindChannel(AlternativeName);
            }
            if (channel == null && Required)
            {
                throw new InvalidDataException($"Material does not have a {Name} channel.");
            }
            return channel;
        }
    }

    private static TextureUniform createTextureUniform(string name, MemoryImage memoryImage, TextureUnit unit)
    {
        var texture = Texture.From(ImageTextureData.From(memoryImage.Open()), t =>
        {
            t.GenerateMipmap();
            t.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
        });
        return new TextureUniform(name, unit, texture);
    }

    private Mesh loadMesh(LogicalMesh mesh, ImmutableDictionary<LogicalMaterial, MeshMaterial> materials)
    {
        if (mesh.Primitives.Count != 1)
        {
            throw new InvalidDataException("Only meshes with a single primitive are supported");
        }

        var primitive = mesh.Primitives[0];
        var material = materials[primitive.Material];

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

        return new Mesh(vertexBuffer, indexBuffer, material);
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
        return buffer;
    }

    private static Buffer<ushort> createIndexBuffer(ushort[] indexArray)
    {
        var buffer = new Buffer<ushort>();
        using var target = buffer.Bind();
        target.Upload(indexArray);
        return buffer;
    }
}
