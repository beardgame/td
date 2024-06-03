using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using MouseEventArgs = Bearded.UI.EventArgs.MouseEventArgs;

namespace Bearded.TD.UI.Controls;

sealed class GameWorldControl : RenderLayerCompositeControl, IDeferredRenderLayer
{
    private readonly GameInstance game;
    private readonly GameRenderer renderer;

    private Matrix4 viewMatrix;
    public override string DebugName => "Game World";
    public override Matrix4 ViewMatrix => viewMatrix;
    public override Matrix4 ProjectionMatrix => game.Camera.ProjectionMatrix;
    public override RenderOptions RenderOptions => RenderOptions.Default;

    public float FarPlaneDistance => game.Camera.FarPlaneDistance;

    public float Time => (float)game.State.Time.NumericValue;

    public float HexagonalFallOffDistance => (game.State.Level.Radius - 0.25f) * Constants.Game.World.HexagonWidth;

    public DeferredContent Content => renderer.Content;

    public GameWorldControl(GameInstance game, RenderContext renderContext, ITimeSource time)
    {
        this.game = game;
        renderer = new GameRenderer(game, renderContext, time);
        UserSettings.SettingsChanged += userSettingsChanged;
        IsClickThrough = false;
    }

    private void userSettingsChanged()
    {
        game.Camera.OnSettingsChanged();
    }

    public override void Draw()
    {
        updateMatrices();
        renderer.Draw();
    }

    private void updateMatrices()
    {
        viewMatrix = game.Camera.ViewMatrix;

        var shake = game.Meta.ScreenShaker.GetDisplacementAt(game.Camera.Position.WithZ(game.Camera.VisibleRadius));

        shake *= UserSettings.Instance.Graphics.ScreenShake;

        viewMatrix *= Matrix4.CreateTranslation(shake.X, shake.Y, 0);
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
        game.Content.Dispose();
        renderer.CleanUp();
        UserSettings.SettingsChanged -= userSettingsChanged;
    }
}
