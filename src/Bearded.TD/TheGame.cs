using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Bearded.Graphics;
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
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.Console;
using Bearded.UI.Controls;
using Bearded.UI.Events;
using Bearded.UI.Navigation;
using Bearded.UI.Rendering;
using Bearded.Utilities.Input;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TextInput = Bearded.TD.UI.Controls.TextInput;
using Window = Bearded.Graphics.Windowing.Window;

namespace Bearded.TD
{
    sealed class TheGame : Window
    {
        private static TheGame? instance;

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
        {
            this.logger = logger;

            instance = this;
        }

        protected override NativeWindowSettings GetSettings() =>
            new()
            {
                Size = new Vector2i(1280, 720),
                API = ContextAPI.OpenGL, APIVersion = new Version(3, 2),
                WindowState = WindowState.Normal
            };

        protected override void OnLoad()
        {
            Serializers<Player, GameInstance>.Initialize();

            ConsoleCommands.Initialize();
            UserSettings.Load(logger);
            UserSettings.Save(logger);

            var dependencyResolver = new DependencyResolver();
            dependencyResolver.Add(logger);

            renderContext = new RenderContext(glActionQueue, logger);

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

            dependencyResolver.Add(renderContext);
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
            navigationController.Push<VersionOverlay>(a =>
                a.Bottom(margin: 4, height: 14).Right(margin: 4, width: 100));
            navigationController.Exited += Close;

            shortcuts.RegisterShortcut(Keys.GraveAccent, debugConsole.Toggle);

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

            if (inputManager.IsKeyPressed(Keys.LeftAlt) && inputManager.IsKeyHit(Keys.F4))
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

#if DEBUG
            if (inputManager.IsKeyHit(Keys.F11))
            {
                trySendScreenshotToDiscord();
            }
#endif
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


        private void trySendScreenshotToDiscord()
        {
            logger.Debug?.Log("Trying to send screenshot to Discord");

            var webhookToken = UserSettings.Instance.Debug.DiscordScreenshotWebhookToken;
            if (webhookToken.IsNullOrEmpty())
            {
                logger.Debug?.Log("Set webhook token to send screenshot to discord");
                return;
            }

            var bitmap = makeScreenshot();

            var webhook = new DiscordWebhook(webhookToken, logger);

            webhook.SendImageInBackground(bitmap);
        }

        private Bitmap makeScreenshot()
        {
            var width = NativeWindow.ClientSize.X;
            var height = NativeWindow.ClientSize.Y;

            var ratio = (float) width / height;

            int x = 0, y = 0, w = width, h = height;

            if (ratio >= 16.0 / 9.0)
            {
                w = (int) (height * 16.0 / 9.0);
                x = (width - w) / 2;
            }
            else
            {
                h = (int) (width * 9.0 / 16.0);
                y = (height - h) / 2;
            }

            var bmp = new Bitmap(w, h);
            var data = bmp.LockBits(
                new Rectangle(0, 0, w, h),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.ReadPixels(x, y, w, h, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }
    }
}
