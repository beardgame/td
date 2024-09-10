using Bearded.Graphics.Rendering;
using Bearded.Graphics.Shading;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Shader = Bearded.TD.Content.Models.Shader;

namespace Bearded.TD.Rendering.Deferred.Level;

sealed class LevelRenderer : IRenderer, IListener<TileDrawInfoChanged>, IListener<ZoneRevealed>
{
    public Heightmap Heightmap { get; }
    private readonly BiomeMaterials biomeMaterials;
    private readonly BiomeBuffer biomeBuffer;
    private readonly HeightRenderer heightRenderer;
    private readonly VisibilityRenderer visibilityRenderer;
    private readonly BiomeRenderer biomeRenderer;
    private readonly IHeightmapToLevelRenderer heightmapToLevelRenderer;

    public LevelRenderer(GameInstance game, RenderContext context, Shader levelShader, ITimeSource time)
    {
        Heightmap = new Heightmap(game);
        heightRenderer = new HeightRenderer(game, context, Heightmap);
        visibilityRenderer = new VisibilityRenderer(game, Heightmap, time);
        biomeBuffer = new BiomeBuffer(game.State.Level.Radius);
        biomeMaterials = new BiomeMaterials(game);
        biomeRenderer = new BiomeRenderer(game, biomeBuffer, biomeMaterials, context);
        heightmapToLevelRenderer = new DualContouredHeightmapToLevelRenderer(
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

    public void SetShaderProgram(ShaderProgram program)
    {
        throw new System.NotImplementedException();
    }

    public void Render()
    {
        heightmapToLevelRenderer.RenderAll();
    }

    public void Dispose()
    {
        Heightmap.Dispose();
        biomeBuffer.Dispose();
        heightRenderer.CleanUp();
        visibilityRenderer.Dispose();
        heightmapToLevelRenderer.Dispose();
    }
}
