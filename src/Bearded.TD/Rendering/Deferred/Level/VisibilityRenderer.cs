using System;
using System.Linq;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Rendering.Deferred.Level
{
    using static Pipeline<Void>;

    sealed class VisibilityRenderer : IDisposable
    {
        private readonly Tiles.Level level;
        private readonly VisibilityLayer visibilityLayer;

        private readonly IPipeline<Void> renderToHeightmap;

        private readonly IndexedTrianglesMeshBuilder<ColorVertexData> meshBuilder;
        private readonly Renderer renderer;
        private readonly ShapeDrawer2<ColorVertexData, Void> drawer;

        private bool needsFullRedraw = true;

        public VisibilityRenderer(GameInstance game, RenderContext context, Heightmap heightmap)
        {
            level = game.State.Level;
            visibilityLayer = game.State.VisibilityLayer;

            heightmap.ResolutionChanged += () => needsFullRedraw = true;

            renderToHeightmap = heightmap.DrawVisibility(
                InOrder(
                    ClearColor(0, 0, 0, 0),
                    Do(renderVisibility)
                ));

            meshBuilder = new IndexedTrianglesMeshBuilder<ColorVertexData>();
            renderer = Renderer.From(meshBuilder.ToRenderable(), heightmap.RadiusUniform);
            drawer = new ShapeDrawer2<ColorVertexData, Void>(meshBuilder, (p, _) => new ColorVertexData(p, default));

            context.Shaders.GetShaderProgram("terrain-base").UseOnRenderer(renderer);
        }

        public void Dispose()
        {
            meshBuilder.Dispose();
            renderer.Dispose();
        }

        public void ZoneChanged(Zone zone)
        {
            // TODO: only redraw what's needed please
            needsFullRedraw = true;
        }

        public void Render()
        {
            if (!needsFullRedraw)
                return;

            renderToHeightmap.Execute();

            needsFullRedraw = false;
        }

        private void renderVisibility()
        {
            var visibleTiles = Tilemap
                .EnumerateTilemapWith(level.Radius)
                .Where(t => visibilityLayer[t].IsVisible());

            var count = 0;
            foreach (var tile in visibleTiles)
            {
                var p = Tiles.Level.GetPosition(tile).NumericValue.WithZ(1);

                drawer.FillCircle(p, Constants.Game.World.HexagonSide * 2, default, 6);

                count++;

                if (count > 10000)
                {
                    renderer.Render();
                    meshBuilder.Clear();
                    count = 0;
                }
            }

            renderer.Render();
            meshBuilder.Clear();
        }
    }
}
