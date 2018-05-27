using System;
using System.ComponentModel;
using amulware.Graphics;
using Bearded.TD.Meta;
using Bearded.TD.Mods;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.UI;
using Bearded.TD.Screens;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities.Console;
using Bearded.UI.Controls;
using Bearded.UI.Events;
using Bearded.UI.Navigation;
using Bearded.UI.Rendering;
using Bearded.Utilities.Input;
using Bearded.Utilities.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace Bearded.TD
{
    class TheGame : Program
    {
        private readonly Logger logger;

        private InputManager inputManager;
        private RenderContext renderContext;
        private ScreenManager screenManager;
        private RootControl rootControl;
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

            contentManager = new ContentManager();
            dependencyResolver.Add(contentManager);

            renderContext = new RenderContext();

            rendererRouter = new CachedRendererRouter(
                new (Type, object)[] {
                    (typeof(Control), new BoxRenderer(renderContext.Surfaces.ConsoleBackground)),
                });

            inputManager = new InputManager(Mouse);

            rootControl = new RootControl();
            eventManager = new EventManager(rootControl, inputManager);
            var uiFactories = UILibrary.CreateFactories(renderContext);
            var navigationController =
                new NavigationController(rootControl, dependencyResolver, uiFactories.models, uiFactories.views);
            navigationController.GoTo<MainMenu>();
            navigationController.Exited += Close;

            screenManager = new ScreenManager(inputManager);
            
            screenManager.AddScreenLayerOnTop(new StartScreen(screenManager, renderContext.Geometries, logger, inputManager, contentManager));
            screenManager.AddScreenLayerOnTop(new ConsoleScreenLayer(screenManager, renderContext.Geometries, logger));
#if DEBUG
            screenManager.AddScreenLayerOnTop(new DebugCoordinatePointerLayer(screenManager, renderContext.Geometries));
#endif

            KeyPress += (sender, args) => screenManager.RegisterPressedCharacter(args.KeyChar);

            UserSettings.SettingsChanged += () => OnResize(null);

            OnResize(EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            var viewportSize = new ViewportSize(Width, Height, UserSettings.Instance.UI.UIScale);
            screenManager.OnResize(viewportSize);
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
            screenManager.Update(e);
        }

        protected override void OnRender(UpdateEventArgs e)
        {
            renderContext.Compositor.PrepareForFrame();
            screenManager.Render(renderContext);
            rootControl.Render(rendererRouter);
            renderContext.Compositor.FinalizeFrame();

            SwapBuffers();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UserSettings.Save(logger);
            base.OnClosing(e);
        }
    }
}
