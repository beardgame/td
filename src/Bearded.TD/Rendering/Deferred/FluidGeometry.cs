using System;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.World;
using Bearded.TD.Game.World.Fluids;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.Rendering
{
    class FluidGeometry
    {
        private static Vector2 rightVector = Direction.Right.Vector();
        
        private readonly int radius;
        private readonly GeometryLayer levelGeometry;
        private readonly Fluid fluid;

        private readonly Tilemap<(float SurfaceLevel, bool HasFluid)> height;
        private readonly Tilemap<Vector2> flow;
        
        private readonly ExpandingVertexSurface<FluidVertex> surface;
        private static Vector2 upRightVector = Direction.UpRight.Vector();
        private static Vector2 upLeftVector = Direction.UpLeft.Vector();

        public FluidGeometry(GameInstance game, Fluid fluid, RenderContext context, Material material)
        {
            radius = game.State.Level.Radius;
            levelGeometry = game.State.GeometryLayer;
            this.fluid = fluid;
            
            height = new Tilemap<(float, bool)>(radius + 1);
            flow = new Tilemap<Vector2>(radius + 1);

            surface = new ExpandingVertexSurface<FluidVertex>()
                .WithShader(material.Shader.SurfaceShader)
                .AndSettings(
                    context.Surfaces.ViewMatrix,
                    context.Surfaces.ProjectionMatrix
                    );
        }

        public void Render()
        {
            resetFlow();
            prepareHeightAndFlow();
            createGeometry();
            
            surface.Render();
        }

        private void resetFlow()
        {
            foreach (var tile in Tilemap.EnumerateTilemapWith(radius))
            {
                flow[tile] = Vector2.Zero;
            }
        }

        private void prepareHeightAndFlow()
        {
            foreach (var tile in Tilemap.EnumerateTilemapWith(radius))
            {
                var (fluidLevel, fluidFlow) = fluid[tile];
                var groundHeight = levelGeometry[tile].DrawInfo.Height;

                if (fluidLevel.NumericValue <= 0.0001f)
                {
                    height[tile] = (groundHeight.NumericValue, false);
                    continue;
                }

                height[tile] = (groundHeight.NumericValue + fluidLevel.NumericValue, true);

                var flowRight = fluidFlow.FlowRight.NumericValue * rightVector;
                var flowUpRight = fluidFlow.FlowUpRight.NumericValue * upRightVector;
                var flowUpLeft = fluidFlow.FlowUpLeft.NumericValue * upLeftVector;

                flow[tile] += flowRight + flowUpRight + flowUpLeft;
                flow[tile.Neighbour(Direction.Right)] -= flowRight;
                flow[tile.Neighbour(Direction.UpRight)] -= flowUpRight;
                flow[tile.Neighbour(Direction.UpLeft)] -= flowUpLeft;
            }
        }

        private void createGeometry()
        {
            foreach (var tile in Tilemap.EnumerateTilemapWith(radius))
            {
                createGeometryForTile(tile);
            }
        }

        private void createGeometryForTile(Tile tile)
        {
            var rightTile = tile.Neighbour(Direction.Right);
            var upRightTile = tile.Neighbour(Direction.UpRight);
            var upLeftTile = tile.Neighbour(Direction.UpLeft);
            
            var current = height[tile];
            var right = height[rightTile];
            var upRight = height[upRightTile];
            var upLeft = height[upLeftTile];

            if (current.HasFluid || right.HasFluid || upRight.HasFluid)
            {
                addTriangle(tile, rightTile, upRightTile, current, right, upRight);
                
            }

            if (current.HasFluid || upRight.HasFluid || upLeft.HasFluid)
            {
                addTriangle(tile, upRightTile, upLeftTile, current, upRight, upLeft);
            }
        }

        private void addTriangle(
            Tile t0, Tile t1, Tile t2,
            (float SurfaceLevel, bool HasFluid) h0,
            (float SurfaceLevel, bool HasFluid) h1,
            (float SurfaceLevel, bool HasFluid) h2)
        {
            var maxValidHeight = getMaxValidHeight(h0, h1, h2);
            
            addTriangle(
                Level.GetPosition(t0).NumericValue.WithZ(h0.HasFluid ? h0.SurfaceLevel : maxValidHeight),
                Level.GetPosition(t1).NumericValue.WithZ(h1.HasFluid ? h1.SurfaceLevel : maxValidHeight),
                Level.GetPosition(t2).NumericValue.WithZ(h2.HasFluid ? h2.SurfaceLevel : maxValidHeight)
            );
        }

        private float getMaxValidHeight(
            (float SurfaceLevel, bool HasFluid) h0,
            (float SurfaceLevel, bool HasFluid) h1,
            (float SurfaceLevel, bool HasFluid) h2)
        {
            var max = h0.HasFluid ? h0.SurfaceLevel : float.NegativeInfinity;
            if (h1.HasFluid)
                max = Math.Max(max, h1.SurfaceLevel);
            if (h2.HasFluid)
                max = Math.Max(max, h2.SurfaceLevel);
            return max;
        }

        private void addTriangle(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            surface.AddVertices(
                new FluidVertex(p0, Vector3.Zero, Vector2.Zero),
                new FluidVertex(p1, Vector3.Zero, Vector2.Zero),
                new FluidVertex(p2, Vector3.Zero, Vector2.Zero)
                );
        }

        public void CleanUp()
        {
            surface.Dispose();
        }
    }
}
