using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Bearded.Audio;
using Bearded.Graphics;
using Bearded.Graphics.Windowing;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.UI;
using Bearded.TD.UI;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.TD.UI.Shortcuts;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.Console;
using Bearded.TD.Utilities.Performance;
using Bearded.UI.Controls;
using Bearded.UI.Events;
using Bearded.UI.Navigation;
using Bearded.Utilities.Input;
using Bearded.Utilities.IO;
using Bearded.Utilities.SpaceTime;
using Bearded.Utilities.Threading;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static Bearded.TD.Constants.Content;
using Activity = Bearded.TD.Utilities.Performance.Activity;
using Image = SixLabors.ImageSharp.Image;
using TimeSpan = System.TimeSpan;
using Window = Bearded.Graphics.Windowing.Window;

namespace Bearded.TD;

sealed class TheGame : Window
{
    private static TheGame? instance;

    private readonly Logger logger;
    private readonly Intent intent;
    private readonly IRenderDoc renderDoc;
    private readonly ManualActionQueue glActionQueue = new();
    private readonly ScreenshotSaver screenshots;
    private readonly ActivityTimer activityTimer;
    private readonly Queue<ImmutableArray<TimedActivity>> recordedActivityTimes = new();

    private InputManager inputManager = null!;
    private RenderContext renderContext = null!;
    private ContentManager contentManager = null!;
    private UIRenderers uiRenderers = null!;
    private RootControl rootControl = null!;
    private UIUpdater uiUpdater = null!;
    private AnimationUpdater uiAnimationUpdater = new ();
    private EventManager eventManager = null!;
    private NavigationController navigationController = null!;
    private GltfPrototype gltfPrototype = null!;

    private ViewportSize viewportSize;

    private readonly TimeSource gameTime = new ();

    public TheGame(Logger logger, Intent intent, IRenderDoc renderDoc)
    {
        this.logger = logger;
        this.intent = intent;
        this.renderDoc = renderDoc;
        screenshots = new ScreenshotSaver(logger, glActionQueue);

        var activityStopwatch = Stopwatch.StartNew();
        activityTimer = new ActivityTimer(() => new Instant(activityStopwatch.Elapsed.TotalSeconds));

        instance = this;
    }

    protected override NativeWindowSettings GetSettings() =>
        new()
        {
            Size = new Vector2i(1280, 720),
            API = ContextAPI.OpenGL,
            APIVersion = new Version(4, 0),
            WindowState = WindowState.Normal,
            Profile = ContextProfile.Core,
            Flags = ContextFlags.ForwardCompatible,
            Icon = createIcon(),
            Title = "Bearded.TD " + Config.VersionString,
        };

    private WindowIcon createIcon()
    {
        using var image = (Image<Rgba32>)Image.Load("assets/icon.png");
        image.DangerousTryGetSinglePixelMemory(out var memory);
        var imageBytes = MemoryMarshal.AsBytes(memory.Span).ToArray();
        var windowIcon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes));
        return windowIcon;
    }

    protected override void OnLoad()
    {
        AudioContext.Initialize();
        Serializers<Player, GameInstance>.Initialize();

        ConsoleCommands.Initialize();
        UserSettings.Load(logger);
        UserSettings.Save(logger);
        UserSettingsSchema.Initialize();

        var renderSettings = new CoreRenderSettings();
        var renderers = new DrawableRenderers(renderSettings);
        renderContext = new RenderContext(glActionQueue, logger, renderers, renderSettings);
        contentManager = new ContentManager(logger, renderContext.GraphicsLoader, new ModLister().GetAll());
        var coreMod = contentManager.ReferenceMod(contentManager.FindMetadata(CoreUI.ModId));
        while (!coreMod.IsLoaded)
        {
            contentManager.Update();
            tryRunQueuedGlActionsFor(TimeSpan.FromMilliseconds(10));
        }

        uiRenderers = new UIRenderers(renderContext, contentManager, coreMod.LoadedMod.Blueprints);
        uiRenderers.Reload();
        contentManager.ModsUnloaded += _ => uiRenderers.Reload();

        var dependencyResolver = new DependencyResolver();
        dependencyResolver.Add(logger);
        dependencyResolver.Add(activityTimer);
        dependencyResolver.Add(
            (IEnumerable<ImmutableArray<TimedActivity>>)recordedActivityTimes.AsReadOnlyEnumerable());
        dependencyResolver.Add(renderContext);
        dependencyResolver.Add(contentManager);

        inputManager = new InputManager(NativeWindow);
        dependencyResolver.Add(inputManager);

        rootControl = new RootControl();
        dependencyResolver.Add(rootControl.FocusManager);

        uiUpdater = new UIUpdater();
        dependencyResolver.Add(uiUpdater);

        var shortcuts = new ShortcutCapturer();
        dependencyResolver.Add(shortcuts);

        var navigationRoot = OnTopCompositeControl.CreateClickThrough("Navigation Root");
        var uiOverlay = OnTopCompositeControl.CreateClickThrough("UI Overlay");

        rootControl.Add(navigationRoot);
        rootControl.Add(uiOverlay);

        var overlayLayer = new OverlayLayer(uiOverlay);
        dependencyResolver.Add(overlayLayer);

        var tooltipFactory = new TooltipFactory(overlayLayer);
        dependencyResolver.Add(tooltipFactory);

        eventManager = new EventManager(rootControl, inputManager, shortcuts);
        var animations = new Animations(gameTime, uiAnimationUpdater);
        var uiFactories = UIFactories.Create(animations, tooltipFactory);
        var uiContext = new UIContext(animations, uiFactories, contentManager);
        var (models, views) = UILibrary.CreateFactories(renderContext, uiContext);
        navigationController =
            new NavigationController(navigationRoot, dependencyResolver, models, views);
        navigationController.Push<MainMenu, Intent>(intent);
        var debugConsole = navigationController.Push<DebugConsole>(a => a.Bottom(relativePercentage: .5));
        navigationController.Push<VersionOverlay>(a =>
            a.Bottom(margin: 4, height: 14).Right(margin: 4, width: 100));
        navigationController.Exited += Close;

        var globalShortcuts = ShortcutLayer.CreateBuilder()
            .AddShortcut(Keys.GraveAccent, debugConsole.Toggle)
            .AddShortcut(Keys.F3, () => togglePerformanceOverlay(logger, null))
#if DEBUG
            .AddShortcut(Keys.F11, () => screenshots.SendScreenshotToDiscordAsync(viewportSize))
#endif
            .AddShortcut(Keys.F12, () => screenshots.SaveScreenShotAsync(viewportSize))
            .Build();
        shortcuts.AddLayer(globalShortcuts);

        renderDoc.ShowOverlay(UserSettings.Instance.Debug.RenderDocOverlay);
        UserSettings.SettingsChanged += () =>
        {
            renderDoc.ShowOverlay(UserSettings.Instance.Debug.RenderDocOverlay);
            TriggerResize();
        };

        if (UserSettings.Instance.Debug.PerformanceOverlay)
            instance!.navigationController.Push<PerformanceOverlay>();

        gltfPrototype = GltfPrototype.Create(renderDoc);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (e.Height == 0 || e.Width == 0)
            return;

        var size = NativeWindow.ClientSize;

        logger.Trace?.Log($"Resizing game window to {size}");

        glActionQueue.Queue(() =>
        {
            viewportSize = new ViewportSize(size.X, size.Y, UserSettings.Instance.UI.UIScale);
            renderContext.OnResize(viewportSize);
            rootControl.SetViewport(size.X, size.Y, UserSettings.Instance.UI.UIScale);
        });
    }

    protected override void OnUpdateUIThread()
    {
        inputManager.ProcessEventsAsync();
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
        recordLastFramesPerformance();
        using var _ = activityTimer.Start(Activity.UpdateGame);

        contentManager.Update();
        inputManager.Update(NativeWindow.IsFocused);

        if (inputManager.IsKeyPressed(Keys.LeftAlt) && inputManager.IsKeyHit(Keys.F4))
        {
            Close();
        }

        eventManager.Update();
        uiUpdater.Update(e);
    }

    private void recordLastFramesPerformance()
    {
        var lastFramesMetrics = activityTimer.Reset(Activity.QuantumFluctuations);

        recordedActivityTimes.Enqueue(lastFramesMetrics);

        if (recordedActivityTimes.Count > 100)
            recordedActivityTimes.Dequeue();
    }

    protected override void OnRender(UpdateEventArgs e)
    {
        using var frameCapture = inputManager.IsKeyHit(Keys.F10) ? renderDoc.CaptureFrame() : null;
        using var discard = activityTimer.Start(Activity.RenderGame);

        gameTime.SetTo(new Instant(e.TimeInS));

        using (activityTimer.Start(Activity.UIAnimations))
        {
            uiAnimationUpdater.Update();
        }

        using (activityTimer.Start(Activity.GLQueueHandler))
        {
            tryRunQueuedGlActionsFor(TimeSpan.FromMilliseconds(16));
        }

        renderContext.Settings.UITime.Value = (float)gameTime.Time.NumericValue;
        renderContext.Compositor.PrepareForFrame();
        rootControl.Render(uiRenderers);
        gltfPrototype.Render();
        renderContext.Compositor.FinalizeFrame();

        using (activityTimer.Start(Activity.SwapBuffer))
        {
            SwapBuffers();
        }
    }

    private void tryRunQueuedGlActionsFor(TimeSpan timeLimit)
    {
        var timer = Stopwatch.StartNew();

        while (glActionQueue.TryExecuteOne())
        {
            if (timer.Elapsed > timeLimit)
                break;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        contentManager.CleanUpAll();
        UserSettings.Save(logger);
    }

    [Command("debug.ui")]
    private static void openUIDebugOverlay(Logger logger, CommandParameters p)
    {
        instance!.navigationController.Push<UIDebugOverlay>();
    }

    [Command("debug.fonts")]
    private static void openFontTextOverlay(Logger logger, CommandParameters p)
    {
        instance!.navigationController.Push<FontTest>();
    }

    [Command("debug.reload-ui-renderers")]
    private static void refreshUI(Logger logger, CommandParameters p)
    {
        instance!.uiRenderers.Reload();
    }

    [Command("perf")]
    private static void togglePerformanceOverlay(Logger logger, CommandParameters? _)
    {
        UserSettings.Instance.Debug.PerformanceOverlay = !UserSettings.Instance.Debug.PerformanceOverlay;
        UserSettings.RaiseSettingsChanged();
        UserSettings.Save(logger);
        if (UserSettings.Instance.Debug.PerformanceOverlay)
            instance!.navigationController.Push<PerformanceOverlay>();
    }

}
