using System;
using Bearded.Graphics;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Camera;

sealed class GameCameraController
{
    private readonly GameCamera camera;
    private readonly float levelRadius;

    private float maxCameraRadius => levelRadius - 3;
    private float maxCameraDistance => maxCameraDistanceOverride ?? levelRadius;

    private float zoomSpeed =>
        Constants.Camera.BaseZoomSpeed * (1 + camera.VisibleRadius * Constants.Camera.ZoomSpeedFactor);

    private Position2? cinematicGoalPosition;
    private float? cinematicGoalDistance;
    private float goalDistance;
    private Position2 scrollAnchor;
    private Vector2 zoomAnchor;
    private bool isGrabbed;
    private float? maxCameraDistanceOverride;

    // Aggregated for the whole frame.
    private Difference2 aggregatedPositionVelocity;
    private float aggregatedDistanceVelocity;
    private float aggregatedDistanceOffset;

    public GameCameraController(GameCamera camera, float levelRadius)
    {
        this.camera = camera;
        this.levelRadius = levelRadius;
        goalDistance = camera.VisibleRadius;
    }

    public void Update(UpdateEventArgs args)
    {
        updatePosition(args);
        updateCameraDistance(args);

        if (!isGrabbed)
        {
            constrictCameraToLevel(args);
        }
    }

    private void updatePosition(UpdateEventArgs args)
    {
        if (isGrabbed) return;

        if (cinematicGoalPosition is { } position)
        {
            moveToCinematicGoalPosition(args, position);
        }
        else
        {
            updatePositionFromAggregatedOffset(args);
        }
    }

    private void moveToCinematicGoalPosition(UpdateEventArgs args, Position2 goalPos)
    {
        var maxEpsilonSquared = 0.01f.U().Squared;

        var error = camera.Position - goalPos;
        var distanceSquared = error.LengthSquared;

        if (distanceSquared <= maxEpsilonSquared)
        {
            camera.Position = goalPos;
            cinematicGoalPosition = null;
        }

        var snapFactor = 1 - MathF.Pow(1e-6f, args.ElapsedTimeInSf);
        camera.Position -= snapFactor * error;
    }

    private void updatePositionFromAggregatedOffset(UpdateEventArgs args)
    {
        var scrollSpeed = Constants.Camera.BaseScrollSpeed * camera.VisibleRadius;
        camera.Position += args.ElapsedTimeInSf * aggregatedPositionVelocity * scrollSpeed;
        aggregatedPositionVelocity = Difference2.Zero;
    }

    private void updateCameraDistance(UpdateEventArgs args)
    {
        if (cinematicGoalDistance is { } distance)
        {
            moveToCinematicGoalDistance(args, distance);
        }
        else
        {
            updateCameraGoalDistance(args);
        }

        moveCameraDistanceToGoalDistance(args);
    }

    private void moveToCinematicGoalDistance(UpdateEventArgs args, float distance)
    {
        var maxEpsilonSquared = .01f.Squared();

        var error = camera.VisibleRadius - distance;
        var distanceSquared = error.Squared();

        if (distanceSquared <= maxEpsilonSquared)
        {
            goalDistance = distance;
            cinematicGoalDistance = null;
        }

        var snapFactor = 1 - MathF.Pow(1e-6f, args.ElapsedTimeInSf);
        goalDistance -= snapFactor * error;
    }

    private void updateCameraGoalDistance(UpdateEventArgs args)
    {
        var newGoalDistance = goalDistance
            + aggregatedDistanceOffset * zoomSpeed
            + aggregatedDistanceVelocity * zoomSpeed * args.ElapsedTimeInSf;

        newGoalDistance = Math.Max(Constants.Camera.FieldOfViewRadiusMin * 0.9f,
            Math.Min(newGoalDistance, maxCameraDistance * 1.1f));

        float error = 0;

        if (newGoalDistance < Constants.Camera.FieldOfViewRadiusMin)
        {
            error = newGoalDistance - Constants.Camera.FieldOfViewRadiusMin;
        }
        else if (newGoalDistance > maxCameraDistance)
        {
            error = newGoalDistance - maxCameraDistance;
        }

        var snapFactor = 1 - MathF.Pow(1e-8f, args.ElapsedTimeInSf);

        newGoalDistance -= error * snapFactor;

        goalDistance = newGoalDistance;

        aggregatedDistanceOffset = 0;
        aggregatedDistanceVelocity = 0;
    }

    private void moveCameraDistanceToGoalDistance(UpdateEventArgs args)
    {
        var error = camera.VisibleRadius - goalDistance;
        var snapFactor = 1 - MathF.Pow(1e-6f, args.ElapsedTimeInSf);
        if (Math.Abs(error) < 0.02f)
        {
            snapFactor = 1;
        }
        var oldZoomAnchorWorldPosition = camera.TransformScreenToWorldPos(zoomAnchor);
        camera.VisibleRadius -= error * snapFactor;

        if (cinematicGoalPosition == null)
        {
            var newZoomAnchorWorldPosition = camera.TransformScreenToWorldPos(zoomAnchor);
            var positionError = newZoomAnchorWorldPosition - oldZoomAnchorWorldPosition;
            camera.Position -= positionError;
        }
    }

    private void constrictCameraToLevel(UpdateEventArgs args)
    {
        var currentMaxCameraRadiusNormalised = 1 - (camera.VisibleRadius / levelRadius).Squared().Clamped(0, 1);
        var currentMaxCameraRadius = maxCameraRadius * currentMaxCameraRadiusNormalised;

        if (camera.Position.NumericValue.LengthSquared <= currentMaxCameraRadius.Squared())
            return;

        var snapBackFactor = 1 - MathF.Pow(0.01f, args.ElapsedTimeInSf);
        var goalPos = new Position2(camera.Position.NumericValue.Normalized() * currentMaxCameraRadius);
        var error = goalPos - camera.Position;
        camera.Position += error * snapBackFactor;
    }

    /// <summary>
    /// Automatically scrolls so the specified world position is in the center.
    /// </summary>
    /// <remarks>Will be interrupted by other scrolling behaviours.</remarks>
    public void ScrollToWorldPos(Position2 worldPos)
    {
        cinematicGoalPosition = worldPos;
    }

    /// <summary>
    /// Automatically scrolls so the specified world position is in the center.
    /// </summary>
    /// <remarks>Will be interrupted by other scrolling behaviours.</remarks>
    public void ScrollToBoundingBox(Position2 topLeft, Position2 bottomRight)
    {
        var size = bottomRight - topLeft;
        cinematicGoalPosition = topLeft + size * 0.5f;

        // TODO: This is all very wishy washy right now based on assumptions that may not hold forever.
        var maxApproxTileRadius = Math.Max(size.Y.NumericValue, size.X.NumericValue);
        goalDistance = maxApproxTileRadius.Clamped(Constants.Camera.FieldOfViewRadiusMin, maxCameraDistance);
    }

    /// <summary>
    /// Sets the current point under the given screen position as an anchor. This can be used together with
    /// <see cref="MoveScrollAnchor"/> to move the camera instantly so that the anchor coincides with a new screen
    /// position;
    /// </summary>
    public void SetScrollAnchor(Vector2 screenPos)
    {
        scrollAnchor = camera.TransformScreenToWorldPos(screenPos);
    }

    /// <summary>
    /// Moves the camera so that the world position stored as anchor in <see cref="SetScrollAnchor"/> is now in the
    /// screen position specified as parameter.
    /// </summary>
    public void MoveScrollAnchor(Vector2 screenPos)
    {
        var currTargetPos = camera.TransformScreenToWorldPos(screenPos);
        var error = currTargetPos - scrollAnchor;
        camera.Position -= error;
    }

    /// <summary>
    /// Indicates the intent to scroll this frame with the specified velocity. May be aggregated with other sources.
    /// </summary>
    /// <remarks>The velocity should not be pre-multiplied with the frame time.</remarks>
    public void Scroll(Difference2 velocity)
    {
        aggregatedPositionVelocity += velocity;
        interruptCinematicMove();
    }

    public void CenterZoomAnchor()
    {
        zoomAnchor = new Vector2(camera.ViewportSize.Width, camera.ViewportSize.Height) * 0.5f;
    }

    /// <summary>
    /// Sets the point in screen space that should remain constant when zooming in and out.
    /// </summary>
    public void SetZoomAnchor(Vector2 screenPos)
    {
        zoomAnchor = screenPos;
    }

    /// <summary>
    /// Indicates the intent to zoom this frame with the specified offset. This offset is frame-independent (for
    /// example the exact delta scroll of a mouse) and may be aggregated with other sources.
    /// </summary>
    public void ConstantZoom(float offset)
    {
        aggregatedDistanceOffset += offset;
        interruptCinematicMove();
    }

    /// <summary>
    /// Indicates the intent to zoom this frame with the specified velocity. May be aggregated with other sources.
    /// </summary>
    /// <remarks>The velocity should not be pre-multiplied with the frame time.</remarks>
    public void Zoom(float velocity)
    {
        aggregatedDistanceVelocity += velocity;
        interruptCinematicMove();
    }

    private void interruptCinematicMove()
    {
        cinematicGoalPosition = null;
        cinematicGoalDistance = null;
    }

    /// <summary>
    /// Grabs the camera, preventing it from being scrolled, except by using the scroll anchor.
    /// </summary>
    public void Grab()
    {
        if (isGrabbed) throw new Exception("Camera can only be grabbed by one thing at the time.");
        isGrabbed = true;
        interruptCinematicMove();
    }

    /// <summary>
    /// Releases the camera, allowing it to be scrolled again.
    /// </summary>
    public void Release()
    {
        if (!isGrabbed) throw new Exception("Cannot release camera if it was never grabbed.");
        isGrabbed = false;
    }

    public void OverrideMaxCameraDistance(float maxDistance)
    {
        maxCameraDistanceOverride = maxDistance;
    }

    public void ResetMaxCameraDistanceOverride()
    {
        maxCameraDistanceOverride = null;
    }
}
