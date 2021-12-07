using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Rendering.Deferred.Level
{
    sealed class LevelRenderer : IListener<TileDrawInfoChanged>, IListener<ZoneRevealed>
    {
        private readonly Heightmap heightmap;
        private readonly HeightRenderer heightRenderer;
        private readonly VisibilityRenderer visibilityRenderer;
        private readonly HeightmapToLevelRenderer heightmapToLevelRenderer;

        public LevelRenderer(
            GameInstance game, RenderContext context, Material material)
        {
            heightmap = new Heightmap(game);
            heightRenderer = new HeightRenderer(game, context, heightmap);
            visibilityRenderer = new VisibilityRenderer(game, context, heightmap);
            heightmapToLevelRenderer = new HeightmapToLevelRenderer(game, context, material, heightmap);

            resizeIfNeeded();
            game.Meta.Events.Subscribe<TileDrawInfoChanged>(this);
            game.Meta.Events.Subscribe<ZoneRevealed>(this);
        }

        public void HandleEvent(TileDrawInfoChanged @event)
        {
            heightRenderer.TileChanged(@event.Tile);
        }

        public void HandleEvent(ZoneRevealed @event)
        {
            visibilityRenderer.ZoneChanged(@event.Zone);
        }

        public void PrepareForRender()
        {
            resizeIfNeeded();
            heightRenderer.Render();
            visibilityRenderer.Render();
        }

        private void resizeIfNeeded()
        {
            var settings = UserSettings.Instance.Graphics;

            heightmap.SetScale(settings.TerrainHeightmapResolution);
            heightmapToLevelRenderer.Resize(settings.TerrainMeshResolution);
        }

        public void Render()
        {
            heightmapToLevelRenderer.RenderAll();
        }

        public void CleanUp()
        {
            heightmap.Dispose();
            heightRenderer.CleanUp();
            visibilityRenderer.Dispose();
            heightmapToLevelRenderer.CleanUp();
        }
    }
}
