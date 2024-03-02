using System;
using Bearded.TD.Meta;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Camera;

sealed class PerspectiveGameCamera : GameCamera
{
    private const float lowestZToRender = -10;
    private const float highestZToRender = 5;

    private float verticalFieldOfViewRadius => UserSettings.Instance.Graphics.FOV.Degrees().Radians * 0.5f;

    private float cameraHeight;
    private float nearPlaneDistance => Math.Max(cameraHeight - highestZToRender, 0.01f);
    public override float FarPlaneDistance => cameraHeight - lowestZToRender;

    protected override void RecalculateMatrices()
    {
        cameraHeight = calculateCameraDistance();
        ViewMatrix = calculateViewMatrix();
        ProjectionMatrix = calculateProjectionMatrix();
    }

    private float calculateCameraDistance()
    {
        return VisibleRadius / MathF.Tan(verticalFieldOfViewRadius);
    }

    private Matrix4 calculateViewMatrix()
    {
        var p = Position.NumericValue;
        var eye = p.WithZ(cameraHeight);
        var target = p.WithZ();
        return Matrix4.LookAt(eye, target, Vector3.UnitY);
    }

    private Matrix4 calculateProjectionMatrix()
    {
        var zNear = nearPlaneDistance;
        var zFar = FarPlaneDistance;

        var yMax = zNear * MathF.Tan(verticalFieldOfViewRadius);
        var xMax = yMax * ViewportSize.AspectRatio;
        var yMin = -yMax;
        var xMin = -xMax;
        return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
    }

    public override Position2 TransformScreenToWorldPos(Vector2 screenPos)
    {
        // This is simple right now under the assumption:
        // * The camera always looks straight down. That is, the camera eye and target both lie
        //   along the infinite extension of cameraPosition in the Z axis.
        return Position + VisibleRadius * GetNormalizedScreenPosition(screenPos);
    }

    public override Vector2 TransformWorldToNormalizedScreenPos(Position2 worldPos)
    {
        return ((worldPos - Position) / VisibleRadius).NumericValue;
    }
}
