using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.Utilities;
using Bearded.Utilities.Math;
using OpenTK;

namespace Bearded.TD.Rendering
{
    class ConsoleScreenLayer : UIScreenLayer
    {
        private const float consoleHeight = 320;
        private const float fontSize = 14;
        private const float lineHeight = 16;
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

        private readonly Logger logger;

        public ConsoleScreenLayer(Logger logger, GeometryManager geometries) : base(geometries, 0, 1, true)
        {
            this.logger = logger;
        }

        public override void Draw()
        {
            Geometries.ConsoleBackground.Color = Color.Black.WithAlpha(.7f).Premultiplied;
            Geometries.ConsoleBackground.DrawRectangle(0, 0, 1280, consoleHeight);

            var logEntries = logger.GetSafeRecentEntries();

            Geometries.ConsoleFont.Height = fontSize;

            var maxVisible = Mathf.CeilToInt(consoleHeight / lineHeight);
            var start = Math.Max(0, logEntries.Count - maxVisible - 1);

            var y = consoleHeight - padding - lineHeight;
            var i = logEntries.Count;

            while (y >= -lineHeight && i > 0)
            {
                var entry = logEntries[--i];
                if (!visibleSeverities.Contains(entry.Severity)) continue;
                Geometries.ConsoleFont.Color = colors[logEntries[i].Severity];
                Geometries.ConsoleFont.DrawString(new Vector2(padding, y), logEntries[i].Text);
                y -= lineHeight;
            }
        }
    }
}
