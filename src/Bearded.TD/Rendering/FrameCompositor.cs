using System.Drawing;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.Utilities.IO;
using OpenTK.Graphics.OpenGL;
using Color = Bearded.Graphics.Color;

namespace Bearded.TD.Rendering;

sealed class FrameCompositor
{
    private readonly DebugOnlyShaderReloader shaderReloader;
    private readonly LayerRenderer layerRenderer;
    public ViewportSize ViewPort { get; private set; }

    private readonly PipelineTexture accumulatedColor;
    private readonly RenderTarget accumulationTarget;

    public FrameCompositor(Logger logger, CoreRenderSettings settings, CoreShaders shaders, CoreRenderers renderers,
        DeferredRenderer deferredRenderer)
    {
        accumulatedColor =  Pipeline.Texture(PixelInternalFormat.Rgba, label: "Layer Accumulation");
        accumulationTarget = RenderTarget.WithColorAttachments(accumulatedColor.Texture);
        renderers.SetLayerAccumulationTexture(accumulatedColor);

        // TODO: use mod specific shader managers and hot reload them separately
        shaderReloader = new DebugOnlyShaderReloader(shaders.ShaderManager, logger);
        layerRenderer = new LayerRenderer(settings, renderers, deferredRenderer);
    }

    public void SetViewportSize(ViewportSize viewPort)
    {
        ViewPort = viewPort;
        layerRenderer.OnResize(viewPort);
    }

    public void PrepareForFrame()
    {
        shaderReloader.ReloadShadersIfNeeded();

        accumulatedColor.EnsureSize((ViewPort.Width, ViewPort.Height));

        using var _ = accumulationTarget.Bind();

        var argb = Color.Black;
        GL.ClearColor(argb.R / 255f, argb.G / 255f, argb.B / 255f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        setGLStateDefaults();
    }

    private void setGLStateDefaults()
    {
        GLState.SetViewport(new Rectangle(0, 0, ViewPort.Width, ViewPort.Height));
    }

    public void RenderLayer(IRenderLayer layer)
    {
        layerRenderer.RenderLayer(layer, accumulationTarget);
    }

    public void FinalizeFrame()
    {
        using var _ = accumulationTarget.Bind(FramebufferTarget.ReadFramebuffer);

        GL.BlitFramebuffer(0, 0, ViewPort.Width, ViewPort.Height, 0, 0, ViewPort.Width, ViewPort.Height,
            ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
    }
}
