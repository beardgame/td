using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using OpenTK.Mathematics;
using MouseEventArgs = Bearded.UI.EventArgs.MouseEventArgs;

namespace Bearded.TD.UI.Controls;

class GameWorldControl : DefaultProjectionRenderLayerControl, IDeferredRenderLayer
{
    private readonly GameRenderer renderer;

    private readonly GameInstance game;

    public override Matrix4 ViewMatrix => game.Camera.ViewMatrix;
    public override Matrix4 ProjectionMatrix => game.Camera.ProjectionMatrix;
    public override RenderOptions RenderOptions => RenderOptions.Default;

    public float FarPlaneDistance => game.Camera.FarPlaneDistance;

    public float Time => (float)game.State.Time.NumericValue;

    public float HexagonalFallOffDistance => (game.State.Level.Radius - 0.25f) * Constants.Game.World.HexagonWidth;

    public ContentRenderers ContentRenderers => renderer.ContentRenderers;

    public GameWorldControl(GameInstance game, RenderContext renderContext, ITimeSource time)
    {
        this.game = game;
        renderer = new GameRenderer(game, renderContext, time);
        UserSettings.SettingsChanged += userSettingsChanged;
    }

    private void userSettingsChanged()
    {
        game.Camera.OnSettingsChanged();
    }

    public override void Draw()
    {
        renderer.Render();
    }

    public override void UpdateViewport(ViewportSize viewport)
    {
        base.UpdateViewport(viewport);
        game.Camera.OnViewportSizeChanged(ViewportSize);
    }

    public override void MouseEntered(MouseEventArgs eventArgs)
    {
        base.MouseEntered(eventArgs);
        game.PlayerInput.Focus();
    }

    public override void MouseExited(MouseEventArgs eventArgs)
    {
        base.MouseExited(eventArgs);
        game.PlayerInput.UnFocus();
    }

    public void CleanUp()
    {
        renderer.CleanUp();
        UserSettings.SettingsChanged -= userSettingsChanged;
    }
}
