using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Rendering.Deferred.Level;
using Bearded.TD.Utilities;

namespace Bearded.TD.Rendering;

sealed class GameRenderer
{
    private readonly GameInstance game;
    private readonly CoreDrawers drawers;
    private readonly LevelRenderer level;
    private readonly DebugGameRenderer debug;

    public DeferredContent Content { get; }

    public GameRenderer(GameInstance game, RenderContext context, ITimeSource time)
    {
        this.game = game;
        drawers = context.Drawers;
        debug = new DebugGameRenderer(game, context);
        var renderers = game.Meta.DrawableRenderers;

        // TODO: this should not stay hardcoded forever
        var levelShader = game.Blueprints.Shaders[ModAwareId.ForDefaultMod("gpu-terrain-tessellated")];
        var waterMaterial = game.Blueprints.Materials[ModAwareId.ForDefaultMod("water")];

        level = new LevelRenderer(game, context, levelShader, time);
        var water = new FluidGeometry(game, game.State.FluidLayer.Water, context, waterMaterial);

        renderers.RegisterRenderer(level, DrawOrderGroup.Level, 0);
        renderers.CreateAndRegisterRenderer(water, DrawOrderGroup.Fluids, 0);

        Content = new DeferredContent(level, renderers);
    }

    public void Draw()
    {
        game.Meta.DrawableRenderers.ClearAll();

        game.PlayerCursors.DrawCursors(drawers);
        drawGameObjects();
        debug.Draw();
        level.PrepareForRender();
    }

    private void drawGameObjects()
    {
        foreach (var renderable in game.State.Enumerate<IRenderable>())
        {
            renderable.Render(drawers);
        }
    }

    public void CleanUp()
    {
        game.Meta.DrawableRenderers.DisposeAll();
    }
}
