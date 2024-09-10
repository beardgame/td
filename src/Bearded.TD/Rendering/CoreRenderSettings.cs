using System;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.UI.Layers;
using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;
using static System.Math;

namespace Bearded.TD.Rendering;

sealed class CoreRenderSettings
{

    public Matrix4Uniform ViewMatrix { get; } = new("view");
    public Matrix4Uniform ProjectionMatrix { get; } = new("projection");
    public FloatUniform FarPlaneDistance { get; } = new("farPlaneDistance");

    public Vector3Uniform FarPlaneBaseCorner { get; } = new("farPlaneBaseCorner");
    public Vector3Uniform FarPlaneUnitX { get; } = new("farPlaneUnitX");
    public Vector3Uniform FarPlaneUnitY { get; } = new("farPlaneUnitY");
    public Vector3Uniform CameraPosition { get; } = new("cameraPosition");

    public FloatUniform Time { get; } = new("time");
    public FloatUniform UITime { get; } = new("uiTime");

    public void SetSettingsFor(IRenderLayer layer)
    {
        ViewMatrix.Value = layer.ViewMatrix;
        ProjectionMatrix.Value = layer.ProjectionMatrix;

        if (layer is IDeferredRenderLayer deferredLayer)
            setDeferredSettings(deferredLayer);
    }

    private void setDeferredSettings(IDeferredRenderLayer deferredRenderLayer)
    {
        FarPlaneDistance.Value = deferredRenderLayer.FarPlaneDistance;
        Time.Value = deferredRenderLayer.Time;

        setFarPlaneParameters(deferredRenderLayer);
    }

    private void setFarPlaneParameters(IRenderLayer renderLayer)
    {
        var projection = renderLayer.ProjectionMatrix;
        var view = renderLayer.ViewMatrix;

        var cameraTranslation = view.ExtractTranslation();
        var viewWithoutTranslation = view.ClearTranslation();

        var projectionView = viewWithoutTranslation * projection;
        var projectionViewInverted = projectionView.Inverted();

        // TODO: check if multiplication by the inverted view matrix is working correctly
        var baseCorner = new Vector4(-1, -1, 1, 1) * projectionViewInverted;
        baseCorner = baseCorner / baseCorner.W;

        var xCorner = new Vector4(1, -1, 1, 1) * projectionViewInverted;
        xCorner = xCorner / xCorner.W;

        var yCorner = new Vector4(-1, 1, 1, 1) * projectionViewInverted;
        yCorner = yCorner / yCorner.W;

        FarPlaneBaseCorner.Value = baseCorner.Xyz;
        FarPlaneUnitX.Value = xCorner.Xyz - baseCorner.Xyz;
        FarPlaneUnitY.Value = yCorner.Xyz - baseCorner.Xyz;
        CameraPosition.Value = cameraTranslation;
    }

    public Rectangle GetCameraFrustumBoundsAtFarPlane()
    {
        var cameraPosition = -CameraPosition.Value;
        var farPlaneBaseCorner = FarPlaneBaseCorner.Value;
        var farPlaneUnitX = FarPlaneUnitX.Value * 2;
        var farPlaneUnitY = FarPlaneUnitY.Value * 2;

        var corner00 = farPlaneBaseCorner.Xy + cameraPosition.Xy;
        var corner10 = corner00 + farPlaneUnitX.Xy;
        var corner01 = corner00 + farPlaneUnitY.Xy;
        var corner11 = corner10 + farPlaneUnitY.Xy;

        var minX = Min(Min(corner00.X, corner10.X), Min(corner01.X, corner11.X));
        var minY = Min(Min(corner00.Y, corner10.Y), Min(corner01.Y, corner11.Y));
        var maxX = Max(Max(corner00.X, corner10.X), Max(corner01.X, corner11.X));
        var maxY = Max(Max(corner00.Y, corner10.Y), Max(corner01.Y, corner11.Y));

        var width = maxX - minX;
        var height = maxY - minY;

        return new Rectangle(minX, minY, width, height);
    }
}
