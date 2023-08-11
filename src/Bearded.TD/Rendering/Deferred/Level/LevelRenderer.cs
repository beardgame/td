using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;

namespace Bearded.TD.Rendering.Deferred.Level;

sealed class LevelRenderer : IListener<TileDrawInfoChanged>, IListener<ZoneRevealed>
{
    public Heightmap Heightmap { get; }
    private readonly BiomeMap biomeMap;
    private readonly HeightRenderer heightRenderer;
    private readonly VisibilityRenderer visibilityRenderer;
    private readonly HeightmapToLevelRenderer heightmapToLevelRenderer;

    public LevelRenderer(
        GameInstance game, RenderContext context, Material material, ITimeSource time)
    {
        Heightmap = new Heightmap(game);
        biomeMap = new BiomeMap(game);
        heightRenderer = new HeightRenderer(game, context, Heightmap);
        visibilityRenderer = new VisibilityRenderer(game, Heightmap, time);
        heightmapToLevelRenderer = new HeightmapToLevelRenderer(game, context, material, Heightmap);

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

        Heightmap.SetScale(settings.TerrainHeightmapResolution);
        heightmapToLevelRenderer.Resize(settings.TerrainMeshResolution);
    }

    public void Render()
    {
        heightmapToLevelRenderer.RenderAll();
    }

    public void CleanUp()
    {
        Heightmap.Dispose();
        biomeMap.Dispose();
        heightRenderer.CleanUp();
        visibilityRenderer.Dispose();
        heightmapToLevelRenderer.CleanUp();
    }
}
