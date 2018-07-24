using System;
using System.ComponentModel;
using System.Diagnostics;
using amulware.Graphics;
using Bearded.TD.Meta;
using Bearded.TD.Mods;
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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using TextInput = Bearded.TD.UI.Controls.TextInput;

namespace Bearded.TD
{
    class TheGame : Program
    {
        private readonly Logger logger;
        private readonly ManualActionQueue glActionQueue = new ManualActionQueue();

        private InputManager inputManager;
        private RenderContext renderContext;
        private RootControl rootControl;
        private UIUpdater uiUpdater;
        private EventManager eventManager;

        private ContentManager contentManager;
        private CachedRendererRouter rendererRouter;
        
        public TheGame(Logger logger)
         : base(1280, 720, GraphicsMode.Default, "Bearded.TD",
             GameWindowFlags.Default, DisplayDevice.Default,
             3, 2, GraphicsContextFlags.Default)
        {
            this.logger = logger;
        }

        protected override void OnLoad(EventArgs e)
        {
            ConsoleCommands.Initialise();
            UserSettings.Load(logger);
            UserSettings.Save(logger);

            var dependencyResolver = new DependencyResolver();
            dependencyResolver.Add(logger);

            contentManager = new ContentManager(glActionQueue);
            dependencyResolver.Add(contentManager);

            renderContext = new RenderContext();

            var surfaces = renderContext.Surfaces;
            rendererRouter = new CachedRendererRouter(
                new (Type, object)[]
                {
                    (typeof(RenderLayerCompositeControl), new RenderLayerCompositeControlRenderer(renderContext.Compositor)),
                    (typeof(AutoCompletingTextInput), new AutoCompletingTextInputRenderer(surfaces.ConsoleBackground, surfaces.ConsoleFontSurface, surfaces.ConsoleFont)),
                    (typeof(TextInput), new TextInputRenderer(surfaces.ConsoleBackground, surfaces.ConsoleFontSurface, surfaces.ConsoleFont)),
                    (typeof(Label), new LabelRenderer(surfaces.ConsoleFontSurface, surfaces.ConsoleFont)),
                    (typeof(Button), new BoxRenderer(surfaces.ConsoleBackground, Color.White)),
                    (typeof(BackgroundBox), new BackgroundBoxRenderer(surfaces.ConsoleBackground)),
                    (typeof(Control), new FallbackBoxRenderer(surfaces.ConsoleBackground)),
                });

            inputManager = new InputManager(this);
            dependencyResolver.Add(inputManager);

            rootControl = new RootControl(new DefaultRenderLayerControl());

            uiUpdater = new UIUpdater();
            dependencyResolver.Add(uiUpdater);
            
            var shortcuts = new ShortcutManager();
            eventManager = new EventManager(rootControl, inputManager, shortcuts);
            var uiFactories = UILibrary.CreateFactories(renderContext);
            var navigationController =
                new NavigationController(rootControl, dependencyResolver, uiFactories.models, uiFactories.views);
            navigationController.Push<MainMenu>();
            var debugConsole = navigationController.Push<DebugConsole>(a => a.Bottom(relativePercentage: .5));
            navigationController.Push<VersionOverlay>(a => a.Bottom(margin: 4, height: 14).Right(margin: 4, width: 100));
            navigationController.Exited += Close;

            shortcuts.RegisterShortcut(Key.Tilde, debugConsole.Toggle);

            UserSettings.SettingsChanged += () => OnResize(null);

            OnResize(EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            var viewportSize = new ViewportSize(Width, Height, UserSettings.Instance.UI.UIScale);
            renderContext.OnResize(viewportSize);
            rootControl.SetViewport(Width, Height, UserSettings.Instance.UI.UIScale);
            base.OnResize(e);
        }

        protected override void OnUpdateUIThread()
        {
            inputManager.ProcessEventsAsync();
        }

        protected override void OnUpdate(UpdateEventArgs e)
        {
            inputManager.Update(Focused);

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
            base.OnClosing(e);
        }
    }
}
