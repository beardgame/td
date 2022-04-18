using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Camera;

abstract class GameCamera
{
    private Position2 position;
    private float visibleRadius;

    public ViewportSize ViewportSize { get; private set; }

    public Position2 Position
    {
        get => position;
        set
        {
            position = value;
            RecalculateMatrices();
        }
    }

    public float VisibleRadius
    {
        get => visibleRadius;
        set
        {
            visibleRadius = value;
            RecalculateMatrices();
        }
    }

    public abstract float FarPlaneDistance { get; }
    public Matrix4 ViewMatrix { get; protected set; }
    public Matrix4 ProjectionMatrix { get; protected set; }

    protected GameCamera()
    {
        ViewportSize = new ViewportSize(1280, 720);
        resetCameraPosition();
    }

    public void OnSettingsChanged()
    {
        RecalculateMatrices();
    }

    private void resetCameraPosition()
    {
        position = Position2.Zero;
        visibleRadius = Constants.Camera.FieldOfViewRadiusDefault;
        RecalculateMatrices();
    }

    protected abstract void RecalculateMatrices();

    public void OnViewportSizeChanged(ViewportSize viewportSize)
    {
        ViewportSize = viewportSize;
    }

    public abstract Position2 TransformScreenToWorldPos(Vector2 screenPos);

    protected Difference2 GetNormalizedScreenPosition(Vector2 screenPos)
    {
        var ret = new Vector2(
            2 * screenPos.X / ViewportSize.Width - 1,
            1 - 2 * screenPos.Y / ViewportSize.Height
        );
        ret.X *= ViewportSize.AspectRatio;
        return new Difference2(ret);
    }
}
