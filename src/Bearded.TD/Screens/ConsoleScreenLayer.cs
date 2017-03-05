using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.Utilities;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Screens
{
    class ConsoleScreenLayer : UIScreenLayer
    {
        private const float consoleHeight = 320;
        private const float inputBoxHeight = 20;
        private const float padding = 6;

        private static readonly Dictionary<Logger.Severity, Color> colors = new Dictionary<Logger.Severity, Color>
        {
            { Logger.Severity.Fatal, Color.DeepPink },
            { Logger.Severity.Error, Color.Red },
            { Logger.Severity.Warning, Color.Yellow },
            { Logger.Severity.Info, Color.White },
            { Logger.Severity.Debug, Color.SpringGreen },
            { Logger.Severity.Trace, Color.SkyBlue },
        };

        private readonly Logger logger;
        private bool isConsoleEnabled;

        private readonly Canvas canvas;
        private readonly ConsoleTextComponent consoleText;

        public ConsoleScreenLayer(Logger logger, GeometryManager geometries) : base(geometries, 0, 1, true)
        {
            this.logger = logger;

            canvas = new Canvas(new ScalingDimension(Screen.X), new FixedSizeDimension(Screen.Y, consoleHeight));
            consoleText = new ConsoleTextComponent(Canvas.Within(canvas, padding, padding, padding + inputBoxHeight, padding));
        }

        public override bool HandleInput(UpdateEventArgs args)
        {
            if (InputManager.IsKeyHit(Key.Tilde))
                isConsoleEnabled = !isConsoleEnabled;

            if (!isConsoleEnabled) return true;

            if (InputManager.IsKeyHit(Key.Enter))
                execute();

            return false;
        }

        public override void Update(UpdateEventArgs args) { }

        private void execute()
        {
            
        }

        public override void Draw()
        {
            if (!isConsoleEnabled) return;

            Geometries.ConsoleBackground.Color = Color.Black.WithAlpha(.7f).Premultiplied;
            Geometries.ConsoleBackground.DrawRectangle(canvas.XStart, canvas.YStart, canvas.Width, canvas.Height);

            consoleText.Draw(Geometries, logger.GetSafeRecentEntries());
        }

        private class ConsoleTextComponent
        {
            private const float fontSize = 14;
            private const float lineHeight = 16;
            
#if DEBUG
            private static readonly HashSet<Logger.Severity> visibleSeverities = new HashSet<Logger.Severity>
            {
                Logger.Severity.Fatal, Logger.Severity.Error, Logger.Severity.Warning,
                Logger.Severity.Info, Logger.Severity.Debug, Logger.Severity.Trace
            };
#else
            private static readonly HashSet<Logger.Severity> visibleSeverities = new HashSet<Logger.Severity>
            {
                Logger.Severity.Fatal, Logger.Severity.Error, Logger.Severity.Warning, Logger.Severity.Info
            };
#endif

            private readonly Canvas canvas;

            public ConsoleTextComponent(Canvas canvas)
            {
                this.canvas = canvas;
            }

            public void Draw(GeometryManager geometries, IReadOnlyList<Logger.Entry> logEntries)
            {
                geometries.ConsoleFont.SizeCoefficient = new Vector2(1, 1);
                geometries.ConsoleFont.Height = fontSize;

                var y = canvas.YEnd - lineHeight;
                var i = logEntries.Count;

                while (y >= -lineHeight && i > 0)
                {
                    var entry = logEntries[--i];
                    if (!visibleSeverities.Contains(entry.Severity)) continue;
                    geometries.ConsoleFont.Color = colors[logEntries[i].Severity];
                    geometries.ConsoleFont.DrawString(new Vector2(padding, y), logEntries[i].Text);
                    y -= lineHeight;
                }
            }
        }
    }
}
