using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    class ConsoleTextComponent : UIComponent
    {
        private static readonly Dictionary<Logger.Severity, Color> colors = new Dictionary<Logger.Severity, Color>
        {
            { Logger.Severity.Fatal, Color.DeepPink },
            { Logger.Severity.Error, Color.Red },
            { Logger.Severity.Warning, Color.Yellow },
            { Logger.Severity.Info, Color.White },
            { Logger.Severity.Debug, Color.SpringGreen },
            { Logger.Severity.Trace, Color.SkyBlue },
        };

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

        private readonly Logger logger;

        public ConsoleTextComponent(Bounds bounds, Logger logger) : base(bounds)
        {
            this.logger = logger;
        }

        public override void Draw(GeometryManager geometries)
        {
            var logEntries = logger.GetSafeRecentEntries();

            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, 1);
            geometries.ConsoleFont.Height = fontSize;

            var y = Bounds.YEnd - lineHeight;
            var i = logEntries.Count;

            while (y >= -lineHeight && i > 0)
            {
                var entry = logEntries[--i];
                if (!visibleSeverities.Contains(entry.Severity)) continue;
                geometries.ConsoleFont.Color = colors[logEntries[i].Severity];
                geometries.ConsoleFont.DrawString(new Vector2(Bounds.XStart, y), logEntries[i].Text);
                y -= lineHeight;
            }
        }
    }
}
