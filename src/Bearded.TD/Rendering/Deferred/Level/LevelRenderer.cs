﻿using Bearded.TD.Content.Models;
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
    private readonly BiomeMaterials biomeMaterials;
    private readonly BiomeBuffer biomeBuffer;
    private readonly HeightRenderer heightRenderer;
    private readonly VisibilityRenderer visibilityRenderer;
    private readonly BiomeRenderer biomeRenderer;
    private readonly HeightmapToLevelRenderer heightmapToLevelRenderer;

    public LevelRenderer(GameInstance game, RenderContext context, Shader levelShader, ITimeSource time)
    {
        Heightmap = new Heightmap(game);
        heightRenderer = new HeightRenderer(game, context, Heightmap);
        visibilityRenderer = new VisibilityRenderer(game, Heightmap, time);
        biomeBuffer = new BiomeBuffer(game.State.Level.Radius);
        biomeMaterials = new BiomeMaterials(game);
        biomeRenderer = new BiomeRenderer(game, biomeBuffer, biomeMaterials, context);
        heightmapToLevelRenderer = new HeightmapToLevelRenderer(
            game, context, Heightmap, biomeBuffer, biomeMaterials, levelShader);

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
        biomeRenderer.Render();
        biomeBuffer.UploadIfNeeded();
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
        biomeBuffer.Dispose();
        heightRenderer.CleanUp();
        visibilityRenderer.Dispose();
        heightmapToLevelRenderer.CleanUp();
    }
}
