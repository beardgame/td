using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Camera;

interface IGameCamera
{
    Matrix4 ViewMatrix { get; }
    Matrix4 ProjectionMatrix { get; }

    void SetViewportSize(ViewportSize viewportSize);
    void SetVisibleArea(Position2 topLeft, Position2 bottomRight, VisibleAreaMode mode = VisibleAreaMode.Fit);

    Position2 ScreenToWorldPosition(Vector2 screenPos);
    Vector2 WorldToScreenPosition(Position2 worldPos);
}

enum VisibleAreaMode
{
    Fit,
    Cover
}
