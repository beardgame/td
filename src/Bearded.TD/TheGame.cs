using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Bearded.Audio;
using Bearded.Graphics;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.UI;
using Bearded.TD.UI;
using Bearded.TD.UI.Controls;
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
using Bearded.UI.Rendering;
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
using Activity = Bearded.TD.Utilities.Performance.Activity;
using Image = SixLabors.ImageSharp.Image;
using TextInput = Bearded.TD.UI.Controls.TextInput;
using TimeSpan = System.TimeSpan;
using Window = Bearded.Graphics.Windowing.Window;

namespace Bearded.TD;

sealed class TheGame : Window
{
    private static TheGame? instance;

    private readonly Logger logger;
    private readonly Intent intent;
    private readonly ManualActionQueue glActionQueue = new();
    private readonly ScreenshotSaver screenshots;
    private readonly ActivityTimer activityTimer;
    private readonly Queue<ImmutableArray<TimedActivity>> recordedActivityTimes = new();

    private InputManager inputManager = null!;
    private RenderContext renderContext = null!;
    private ContentManager contentManager = null!;
    private RootControl rootControl = null!;
    private UIUpdater uiUpdater = null!;
    private EventManager eventManager = null!;

    private CachedRendererRouter rendererRouter = null!;
    private NavigationController navigationController = null!;

    private ViewportSize viewportSize;

    public TheGame(Logger logger, Intent intent)
    {
        this.logger = logger;
        this.intent = intent;
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

        renderContext = new RenderContext(glActionQueue, logger);
        contentManager = new ContentManager(logger, renderContext.GraphicsLoader, new ModLister().GetAll());

        var drawers = renderContext.Drawers;
        rendererRouter = new CachedRendererRouter(
            new (Type, object)[]
            {
                (typeof(UIDebugOverlayControl.Highlight),
                    new UIDebugOverlayHighlightRenderer(drawers.ConsoleBackground, drawers.ConsoleFont)),
                (typeof(RenderLayerCompositeControl),
                    new RenderLayerCompositeControlRenderer(renderContext.Compositor)),
                (typeof(AutoCompletingTextInput),
                    new AutoCompletingTextInputRenderer(drawers.ConsoleBackground, drawers.UIFont)),
                (typeof(TextInput), new TextInputRenderer(drawers.ConsoleBackground, drawers.UIFont)),
                (typeof(Label), new LabelRenderer(drawers.UIFont)),
                (typeof(Border), new BorderRenderer(drawers.ConsoleBackground)),
                (typeof(BackgroundBox), new BackgroundBoxRenderer(drawers.ConsoleBackground)),
                (typeof(ButtonBackgroundEffect), new ButtonBackgroundEffectRenderer(drawers.ConsoleBackground)),
                (typeof(Dot), new DotRenderer(drawers.ConsoleBackground)),
                (typeof(Control), new FallbackBoxRenderer(drawers.ConsoleBackground)),
            });

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

        var navigationRoot = new OnTopCompositeControl();
        var uiOverlay = new OnTopCompositeControl();

        rootControl.Add(navigationRoot);
        rootControl.Add(uiOverlay);

        eventManager = new EventManager(rootControl, inputManager, shortcuts);
        var (models, views) = UILibrary.CreateFactories(renderContext);
        navigationController =
            new NavigationController(navigationRoot, dependencyResolver, models, views);
        navigationController.Push<MainMenu, Intent>(intent);
        var debugConsole = navigationController.Push<DebugConsole>(a => a.Bottom(relativePercentage: .5));
        navigationController.Push<VersionOverlay>(a =>
            a.Bottom(margin: 4, height: 14).Right(margin: 4, width: 100));
        navigationController.Exited += Close;

        var overlayLayer = new OverlayLayer(uiOverlay);
        dependencyResolver.Add(overlayLayer);
        var tooltipFactory = new TooltipFactory(overlayLayer);
        dependencyResolver.Add(tooltipFactory);

        var globalShortcuts = ShortcutLayer.CreateBuilder()
            .AddShortcut(Keys.GraveAccent, debugConsole.Toggle)
            .AddShortcut(Keys.F3, () => togglePerformanceOverlay(logger, null))
#if DEBUG
            .AddShortcut(Keys.F11, () => screenshots.SendScreenshotToDiscordAsync(viewportSize))
#endif
            .AddShortcut(Keys.F12, () => screenshots.SaveScreenShotAsync(viewportSize))
            .Build();
        shortcuts.AddLayer(globalShortcuts);

        UserSettings.SettingsChanged += TriggerResize;

        if (UserSettings.Instance.Debug.PerformanceOverlay)
            instance!.navigationController.Push<PerformanceOverlay>();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (e.Height == 0 || e.Width == 0)
            return;

        var size = NativeWindow.ClientSize;

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
        using var discard = activityTimer.Start(Activity.RenderGame);

        using (activityTimer.Start(Activity.GLQueueHandler))
        {
            tryRunQueuedGlActionsFor(TimeSpan.FromMilliseconds(16));
        }

        renderContext.Compositor.PrepareForFrame();
        rootControl.Render(rendererRouter);
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
        UserSettings.Save(logger);
    }

    [Command("debug.ui")]
    private static void openUIDebugOverlay(Logger logger, CommandParameters p)
    {
        instance!.navigationController.Push<UIDebugOverlay>();
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
