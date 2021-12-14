using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Rendering.Deferred.Level;

sealed class LevelRenderer : IListener<TileDrawInfoChanged>, IListener<ZoneRevealed>
{
    public Heightmap Heightmap { get; }
    private readonly HeightRenderer heightRenderer;
    private readonly VisibilityRenderer visibilityRenderer;
    private readonly HeightmapToLevelRenderer heightmapToLevelRenderer;

    public LevelRenderer(
        GameInstance game, RenderContext context, Material material)
    {
        Heightmap = new Heightmap(game);
        heightRenderer = new HeightRenderer(game, context, Heightmap);
        visibilityRenderer = new VisibilityRenderer(game, context, Heightmap);
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
        heightRenderer.CleanUp();
        visibilityRenderer.Dispose();
        heightmapToLevelRenderer.CleanUp();
    }
}