using System;
using System.ComponentModel;
using System.Diagnostics;
using amulware.Graphics;
using amulware.Graphics.Windowing;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.UI;
using Bearded.TD.UI;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Console;
using Bearded.UI.Controls;
using Bearded.UI.Events;
using Bearded.UI.Navigation;
using Bearded.UI.Rendering;
using Bearded.Utilities.Input;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using TextInput = Bearded.TD.UI.Controls.TextInput;

namespace Bearded.TD
{
    sealed class TheGame : Window
    {
        private static TheGame instance;

        private readonly Logger logger;
        private readonly ManualActionQueue glActionQueue = new ManualActionQueue();

        private InputManager inputManager;
        private RenderContext renderContext;
        private RootControl rootControl;
        private UIUpdater uiUpdater;
        private EventManager eventManager;

        private CachedRendererRouter rendererRouter;
        private NavigationController navigationController;

        public TheGame(Logger logger)
         : base(
             new NativeWindowSettings
                 {
                     Size = new Vector2i(1280, 720),
                     API = ContextAPI.OpenGL, APIVersion = new Version(3, 2),
                     WindowState = WindowState.Normal
                 }
             )
        {
            this.logger = logger;

            instance = this;
        }

        protected override void OnLoad()
        {
            Serializers<Player, GameInstance>.Initialize();

            ConsoleCommands.Initialise();
            UserSettings.Load(logger);
            UserSettings.Save(logger);

            var dependencyResolver = new DependencyResolver();
            dependencyResolver.Add(logger);

            renderContext = new RenderContext(glActionQueue, logger);

            var surfaces = renderContext.Surfaces;
            rendererRouter = new CachedRendererRouter(
                new (Type, object)[]
                {
                    (typeof(UIDebugOverlayControl.Highlight), new UIDebugOverlayHighlightRenderer(surfaces.ConsoleBackground, surfaces.ConsoleFontSurface, surfaces.ConsoleFont)),
                    (typeof(RenderLayerCompositeControl), new RenderLayerCompositeControlRenderer(renderContext.Compositor)),
                    (typeof(AutoCompletingTextInput), new AutoCompletingTextInputRenderer(surfaces.ConsoleBackground, surfaces.ConsoleFontSurface, surfaces.ConsoleFont)),
                    (typeof(TextInput), new TextInputRenderer(surfaces.ConsoleBackground, surfaces.ConsoleFontSurface, surfaces.ConsoleFont)),
                    (typeof(Label), new LabelRenderer(surfaces.ConsoleFontSurface, surfaces.ConsoleFont)),
                    (typeof(Border), new BoxRenderer(surfaces.ConsoleBackground, Color.White)),
                    (typeof(BackgroundBox), new BackgroundBoxRenderer(surfaces.ConsoleBackground)),
                    (typeof(ButtonBackgroundEffect), new ButtonBackgroundEffectRenderer(surfaces.ConsoleBackground)),
                    (typeof(Dot), new DotRenderer(surfaces.ConsoleBackground)),
                    (typeof(Control), new FallbackBoxRenderer(surfaces.ConsoleBackground)),
                });

            dependencyResolver.Add(renderContext.GraphicsLoader);

            inputManager = new InputManager(NativeWindow);
            dependencyResolver.Add(inputManager);

            rootControl = new RootControl(new DefaultRenderLayerControl());
            dependencyResolver.Add(rootControl.FocusManager);

            uiUpdater = new UIUpdater();
            dependencyResolver.Add(uiUpdater);

            var shortcuts = new ShortcutManager();
            dependencyResolver.Add(shortcuts);

            eventManager = new EventManager(rootControl, inputManager, shortcuts);
            var (models, views) = UILibrary.CreateFactories(renderContext);
            navigationController =
                new NavigationController(rootControl, dependencyResolver, models, views);
            navigationController.Push<MainMenu>();
            var debugConsole = navigationController.Push<DebugConsole>(a => a.Bottom(relativePercentage: .5));
            navigationController.Push<VersionOverlay>(a => a.Bottom(margin: 4, height: 14).Right(margin: 4, width: 100));
            navigationController.Exited += Close;

            shortcuts.RegisterShortcut(Key.Tilde, debugConsole.Toggle);

            UserSettings.SettingsChanged += TriggerResize;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            var viewportSize = new ViewportSize(e.Width, e.Height, UserSettings.Instance.UI.UIScale);
            renderContext.OnResize(viewportSize);
            rootControl.SetViewport(e.Width, e.Height, UserSettings.Instance.UI.UIScale);
        }

        protected override void OnUpdateUIThread()
        {
            inputManager.ProcessEventsAsync();
        }

        protected override void OnUpdate(UpdateEventArgs e)
        {
            inputManager.Update(NativeWindow.IsFocused);

            if (inputManager.IsKeyPressed(Key.AltLeft) && inputManager.IsKeyHit(Key.F4))
            {
                Close();
            }

            eventManager.Update();
            uiUpdater.Update(e);
        }

        protected override void OnRender(UpdateEventArgs e)
        {
            tryRunQueuedGlActionsFor(TimeSpan.FromMilliseconds(16));

            renderContext.Compositor.PrepareForFrame();
            rootControl.Render(rendererRouter);
            renderContext.Compositor.FinalizeFrame();

            SwapBuffers();
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
            instance.navigationController.Push<UIDebugOverlay>();
        }
    }
}
