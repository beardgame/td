using Bearded.Graphics;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Camera;

interface IGameCameraController
{
    void Update(UpdateEventArgs args);

    /// <summary>
    /// Automatically scrolls so the specified world position is in the center.
    /// </summary>
    /// <remarks>Will be interrupted by other scrolling behaviours.</remarks>
    void ScrollToWorldPos(Position2 worldPos);

    /// <summary>
    /// Automatically scrolls so the specified world position is in the center.
    /// </summary>
    /// <remarks>Will be interrupted by other scrolling behaviours.</remarks>
    void ScrollToBoundingBox(Position2 topLeft, Position2 bottomRight);

    /// <summary>
    /// Sets the current point under the given screen position as an anchor. This can be used together with
    /// <see cref="MoveScrollAnchor"/> to move the camera instantly so that the anchor coincides with a new screen
    /// position;
    /// </summary>
    void SetScrollAnchor(Vector2 screenPos);

    /// <summary>
    /// Moves the camera so that the world position stored as anchor in <see cref="SetScrollAnchor"/> is now in the
    /// screen position specified as parameter.
    /// </summary>
    void MoveScrollAnchor(Vector2 screenPos);

    /// <summary>
    /// Indicates the intent to scroll this frame with the specified velocity. May be aggregated with other sources.
    /// </summary>
    /// <remarks>The velocity should not be pre-multiplied with the frame time.</remarks>
    void Scroll(Difference2 velocity);

    void CenterZoomAnchor();

    /// <summary>
    /// Sets the point in screen space that should remain constant when zooming in and out.
    /// </summary>
    void SetZoomAnchor(Vector2 screenPos);

    /// <summary>
    /// Indicates the intent to zoom this frame with the specified offset. This offset is frame-independent (for
    /// example the exact delta scroll of a mouse) and may be aggregated with other sources.
    /// </summary>
    void ConstantZoom(float offset);

    /// <summary>
    /// Indicates the intent to zoom this frame with the specified velocity. May be aggregated with other sources.
    /// </summary>
    /// <remarks>The velocity should not be pre-multiplied with the frame time.</remarks>
    void Zoom(float velocity);

    /// <summary>
    /// Grabs the camera, preventing it from being scrolled, except by using the scroll anchor.
    /// </summary>
    void Grab();

    /// <summary>
    /// Releases the camera, allowing it to be scrolled again.
    /// </summary>
    void Release();

    void OverrideMaxCameraDistance(float maxDistance);
    void ResetMaxCameraDistanceOverride();
}
