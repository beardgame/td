using Bearded.Graphics.RenderSettings;
using Bearded.TD.UI.Layers;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering
{
    sealed class CoreRenderSettings
    {

        public Matrix4Uniform ViewMatrix { get; } = new("view");
        public Matrix4Uniform ViewMatrixLevel { get; } = new("view");
        public Matrix4Uniform ProjectionMatrix { get; } = new("projection");
        public FloatUniform FarPlaneDistance { get; } = new("farPlaneDistance");

        public Vector3Uniform FarPlaneBaseCorner { get; } = new("farPlaneBaseCorner");
        public Vector3Uniform FarPlaneUnitX { get; } = new("farPlaneUnitX");
        public Vector3Uniform FarPlaneUnitY { get; } = new("farPlaneUnitY");
        public Vector3Uniform CameraPosition { get; } = new("cameraPosition");

        public FloatUniform Time { get; } = new("time");

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
    }
}
