using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bearded.Graphics;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Shader = Bearded.TD.Content.Models.Shader;
using TimeSpan = System.TimeSpan;

namespace Bearded.TD.Rendering.Deferred.Level;

sealed partial class DualContouredHeightmapToLevelRenderer : IHeightmapToLevelRenderer
{
    private sealed class Cell(
        Box3 boundingBox,
        Vector3i subdivision,
        IRenderer renderable,
        Buffer<LevelVertex> vertices,
        Buffer<ushort> indices)
        : IDisposable
    {
        public Box3 BoundingBox { get; } = boundingBox;
        public Vector3i Subdivision { get; } = subdivision;

        public BufferStream<LevelVertex> Vertices { get; } = new(vertices);
        public BufferStream<ushort> Indices { get; } = new(indices);

        private bool isDirty;

        private readonly IReadOnlyCollection<IDisposable> disposables = [ renderable, vertices, indices ];

        public static Cell From(
            Box3 boundingBox, Vector3i subdivision,
            Shader shader,
            IEnumerable<IRenderSetting> settings)
        {
            var vertices = new Buffer<LevelVertex>();
            var indices = new Buffer<ushort>();

            var renderable = Renderable.Build(PrimitiveType.Triangles, b => b
                .With(vertices.AsVertexBuffer())
                .With(indices.AsIndexBuffer())
            );

            var renderer = Renderer.From(renderable, settings);
            shader.RendererShader.UseOnRenderer(renderer);

            return new Cell(boundingBox, subdivision, renderer, vertices, indices);
        }

        public void QueueFlushOnNextRender()
        {
            isDirty = true;
        }

        public void Render()
        {
            if (isDirty)
            {
                Vertices.FlushIfDirty();
                Indices.FlushIfDirty();
                isDirty = false;
            }
            renderable.Render();
        }

        public void Dispose()
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }

    private readonly GameInstance game;
    private readonly CoreRenderSettings settings;
    private readonly Shader levelShader;
    private readonly IReadOnlyCollection<IRenderSetting> renderSettings;

    private readonly FloatUniform heightScaleUniform = new("heightScale");
    private readonly FloatUniform heightOffsetUniform = new("heightOffset");

    private readonly Dictionary<Vector2, Cell> cells = new();
    private HeightmapToLevelRendererHelpers.Grid grid;

    const int gridMinZ = -10;
    const int gridMaxZ = 2;
    private const int cellRes = 64;
    private const float cellSize = 6;
    static readonly Vector3i defaultSubdivision = (cellRes, cellRes, cellRes);

    private readonly List<Cell> cellsToGenerate = [];
    private readonly List<(Cell Cell, Task<TimeSpan> Task)> cellGenerationTasks = [];
    private readonly HashSet<Cell> cellsBeingGenerated = [];

    public DualContouredHeightmapToLevelRenderer(
        GameInstance game,
        RenderContext context,
        Heightmap heightmap,
        BiomeBuffer biomeBuffer,
        BiomeMaterials biomeMaterials,
        Shader levelShader)
    {
        this.game = game;
        settings = context.Settings;
        this.levelShader = levelShader;

        renderSettings = [
            context.Settings.ViewMatrix,
            context.Settings.ProjectionMatrix,
            context.Settings.FarPlaneDistance,
            heightmap.RadiusUniform,
            heightmap.PixelSizeUVUniform,
            heightmap.GetMapTextureUniform("heightmap", TextureUnit.Texture0),
            biomeBuffer.GetTextureUniform("biomeTilemap", TextureUnit.Texture0 + 1),
            biomeBuffer.GetRadiusUniform("biomeTilemapRadius"),
            context.Settings.CameraPosition,
            heightScaleUniform,
            heightOffsetUniform,
            ..biomeMaterials.Samplers.Select((s, i) => s.GetUniform(TextureUnit.Texture0 + i + 2)),
        ];

        var level = game.State.Level;

        var cellWidthCandidate = cellSize * Constants.Game.World.HexagonDistanceX;

        var tilingX = new Vector2(cellWidthCandidate, 0);
        var tilingY = new Vector2(0, cellWidthCandidate);

        grid = HeightmapToLevelRendererHelpers.Grid.For(level, 1, tilingX, tilingY);
    }

    public void RenderAll()
    {
        if (UserSettings.Instance.Debug.WireframeLevel)
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            render();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }
        else
        {
            render();
        }
    }

    private void render()
    {
        var bounds = settings.GetCameraFrustumBoundsAtFarPlane();

        heightScaleUniform.Value = 1;
        heightOffsetUniform.Value = 0;
        grid.IterateCellsIn(bounds, offset =>
        {
            getCell(offset).Render();
        });

        /*
        GL.FrontFace(FrontFaceDirection.Cw);
        heightScaleUniform.Value = -1;
        heightOffsetUniform.Value = 1.5f;
        grid.IterateCellsIn(bounds, offset =>
        {
            return;
            getCell(offset).Render();
        });
        GL.FrontFace(FrontFaceDirection.Ccw);
*/
        updateCellGeneration();
    }

    private void updateCellGeneration()
    {
        foreach (var (_, task) in cellGenerationTasks)
        {
            if (task.IsCompletedSuccessfully)
            {
                //game.Meta.Logger.Trace?.Log(
                //    $"Terrain cell generation task completed successfully in {task.Result.TotalMilliseconds:0.0}ms.");
            }
            if (task.IsFaulted)
            {
                game.Meta.Logger.Warning?.Log(
                    $"Terrain cell generation task failed with: {task.Exception}");
            }
        }

        cellGenerationTasks.RemoveAll(t =>
        {
            var remove = t.Task.IsCompleted;
            if (remove)
            {
                cellsBeingGenerated.Remove(t.Cell);
            }
            return remove;
        });

        var cameraPosition = settings.CameraPosition.Value.Xy;
        cellsToGenerate.Sort(
            (c1, c2) =>
            {
                var d1 = (-c1.BoundingBox.Center.Xy - cameraPosition).LengthSquared;
                var d2 = (-c2.BoundingBox.Center.Xy - cameraPosition).LengthSquared;
                return d2.CompareTo(d1);
            });

        while (cellsToGenerate.Count > 0 && cellGenerationTasks.Count < 8)
        {
            var cell = cellsToGenerate.Last();
            cellsToGenerate.RemoveAt(cellsToGenerate.Count - 1);
            var task = Task.Run(() =>
            {
                var stopWatch = System.Diagnostics.Stopwatch.StartNew();
                fillCellBuffers(cell);
                return stopWatch.Elapsed;
            });
            cellGenerationTasks.Add((cell, task));
        }
    }

    private Cell getCell(Vector2 offset)
    {
        if (!cells.TryGetValue(offset, out var cell))
        {
            var gridCellSize = new Vector3(grid.TilingX.X, grid.TilingY.Y, gridMaxZ - gridMinZ);
            var min = offset.WithZ(gridMinZ);
            var max = min + gridCellSize;
            var boundingBox = new Box3(min, max);

            cell = Cell.From(boundingBox, defaultSubdivision, levelShader, renderSettings);

            cells[offset] = cell;

            cellsToGenerate.Add(cell);
            cellsBeingGenerated.Add(cell);
        }

        return cell;
    }

    public void Dispose()
    {
        foreach (var cell in cells.Values)
        {
            cell.Dispose();
        }
    }

    public void Resize(float scale)
    {
    }
}
