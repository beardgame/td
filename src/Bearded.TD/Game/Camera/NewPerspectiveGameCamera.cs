using System;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Camera;

sealed class NewPerspectiveGameCamera : IGameCamera
{
    private const float lowestZToRender = -5;
    private const float highestZToRender = 5;
    private const float fovy = MathConstants.PiOver2;

    private float nearPlaneDistance => Math.Max(distance - highestZToRender, 0.1f);
    private float farPlaneDistance => distance - lowestZToRender;

    private ViewportSize viewportSize;
    private Position2 position;
    private float distance;

    public Matrix4 ViewMatrix { get; private set; }
    public Matrix4 ProjectionMatrix { get; private set; }

    public NewPerspectiveGameCamera()
    {
        viewportSize = new ViewportSize(1280, 720);
        resetCameraPosition();
    }

    public void SetViewportSize(ViewportSize viewportSize)
    {
        this.viewportSize = viewportSize;
    }

    public void SetVisibleArea(Position2 topLeft, Position2 bottomRight, VisibleAreaMode mode = VisibleAreaMode.Fit)
    {
        position = topLeft + 0.5f * (bottomRight - topLeft);

        var width = bottomRight.X - topLeft.X;
        var height = bottomRight.Y - topLeft.Y;
        var areaAspectRatio = Math.Abs(width / height);
        var areaIsWiderThanViewport = areaAspectRatio > viewportSize.AspectRatio;

        // This is simple right now under the assumptions:
        // * The camera always looks straight down. That is, the camera eye and target both lie
        //   along the infinite extension of cameraPosition in the Z axis.
        // * The FoV is Pi/2
        // Changing the FoV would require trigonometry.
        switch (mode, areaIsWiderThanViewport)
        {
            case (VisibleAreaMode.Fit, true):
            case (VisibleAreaMode.Cover, false):
                // Align the width of the area with the viewport width.
                distance = 0.5f * Math.Abs(width.NumericValue / viewportSize.AspectRatio);
                break;
            case (VisibleAreaMode.Fit, false):
            case (VisibleAreaMode.Cover, true):
                // Align the height of the area with the viewport height.
                distance = 0.5f * Math.Abs(height.NumericValue);
                break;
        }
    }

    private void resetCameraPosition()
    {
        position = Position2.Zero;
        distance = Constants.Camera.ZDefault;
        recalculateMatrices();
    }

    private void recalculateMatrices()
    {
        ViewMatrix = calculateViewMatrix();
        ProjectionMatrix = calculateProjectionMatrix();
    }

    private Matrix4 calculateViewMatrix()
    {
        var p = position.NumericValue;
        var eye = p.WithZ(distance);
        var target = p.WithZ();
        return Matrix4.LookAt(eye, target, Vector3.UnitY);
    }

    private Matrix4 calculateProjectionMatrix()
    {
        var zNear = nearPlaneDistance;
        var zFar = farPlaneDistance;

        var yMax = zNear * MathF.Tan(.5f * fovy);
        var yMin = -yMax;
        var xMax = yMax * viewportSize.AspectRatio;
        var xMin = yMin * viewportSize.AspectRatio;
        return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
    }

    public Position2 ScreenToWorldPosition(Vector2 screenPos)
    {
        // This is simple right now under the assumptions:
        // * The camera always looks straight down. That is, the camera eye and target both lie
        //   along the infinite extension of cameraPosition in the Z axis.
        // * The FoV is Pi/2
        return position + distance * screenToNormalizedScreenPosition(screenPos);
    }

    private Difference2 screenToNormalizedScreenPosition(Vector2 screenPos)
    {
        var ret = new Vector2(
            2 * screenPos.X / viewportSize.Width - 1,
            1 - 2 * screenPos.Y / viewportSize.Height
        );
        ret.X *= viewportSize.AspectRatio;
        return new Difference2(ret);
    }

    public Vector2 WorldToScreenPosition(Position2 worldPos)
    {
        return (worldPos.NumericValue.WithZw(0, 1) * ViewMatrix * ProjectionMatrix).Xy;
    }
}
