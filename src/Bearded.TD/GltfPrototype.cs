using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Windowing;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Mathematics;
using SharpGLTF.Schema2;

namespace Bearded.TD;

sealed class GltfPrototype
{
    public static GltfPrototype Create(IRenderDoc renderDoc)
    {
        var modelRoot = ModelRoot.Load("assets/mods/debug/gfx/models/truck.gltf");
        var truckChassis = modelRoot.LogicalMeshes.First(m => m.Name == "Cube");
        var primitive = truckChassis.Primitives[0];

        List<UVColorVertex> vertices = [];
        List<uint> indices = [];

        var positions = primitive.GetVertexAccessor("POSITION").AsVector3Array();
        var uvs = primitive.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
        var normals = primitive.GetVertexAccessor("NORMAL").AsVector3Array();

        for (var i = 0; i < positions.Count; i++)
        {
            vertices.Add(new UVColorVertex(vec3(positions[i]), vec2(uvs[i]), Color.White));
        }
        foreach (var index in primitive.GetIndices())
        {
            indices.Add(index);
        }

        using var frameCapture = renderDoc.CaptureFrame();

        return new GltfPrototype(vertices, indices);
    }

    private readonly IList<UVColorVertex> vertices;
    private readonly IList<uint> indices;

    private readonly Buffer<UVColorVertex> vertexBuffer;
    private readonly Buffer<uint> indexBuffer;

    private GltfPrototype(IList<UVColorVertex> vertices, IList<uint> indices)
    {
        this.vertices = vertices;
        this.indices = indices;

        vertexBuffer = new Buffer<UVColorVertex>();
        indexBuffer = new Buffer<uint>();

        using (var target = vertexBuffer.Bind())
        {
            target.Upload(vertices.ToArray());
        }
        using (var target = indexBuffer.Bind())
        {
            target.Upload(indices.ToArray());
        }

        //var renderable = Renderable.ForVerticesAndIndices(vertexBuffer, indexBuffer, PrimitiveType.Triangles);
    }

    public void Render()
    {
    }

    private static Vector2 vec2(System.Numerics.Vector2 v) => new(v.X, v.Y);
    private static Vector3 vec3(System.Numerics.Vector3 v) => new(v.X, v.Y, v.Z);
}
