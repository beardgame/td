using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Rendering.Deferred.Level
{
    sealed class LevelRenderer : IListener<TileDrawInfoChanged>
    {
        private readonly HeightmapRenderer heightmapRenderer;
        private readonly HeightmapToLevelRenderer heightmapToLevelRenderer;

        public LevelRenderer(
            GameInstance game, RenderContext context, Material material)
        {
            heightmapRenderer = new HeightmapRenderer(game, context);
            heightmapToLevelRenderer = new HeightmapToLevelRenderer(game, context, material, heightmapRenderer);

            resizeIfNeeded();
            game.Meta.Events.Subscribe(this);
        }

        public void HandleEvent(TileDrawInfoChanged @event)
        {
            heightmapRenderer.TileChanged(@event.Tile);
        }

        public void PrepareForRender()
        {
            heightmapRenderer.EnsureHeightmapIsUpToDate();
        }

        public void RenderAll()
        {
            resizeIfNeeded();

            heightmapToLevelRenderer.RenderAll();
        }

        private void resizeIfNeeded()
        {
            var settings = UserSettings.Instance.Graphics;

            heightmapRenderer.SetScale(settings.TerrainHeightmapResolution);
            heightmapToLevelRenderer.Resize(settings.TerrainMeshResolution);
        }

        public void CleanUp()
        {
            heightmapRenderer.CleanUp();
            heightmapToLevelRenderer.CleanUp();
        }
    }
}
