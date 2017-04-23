using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.Utilities;

namespace Bearded.TD.UI.Components
{
    class ConsoleTextBox : TextBox<Logger.Entry>
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
        
        public ConsoleTextBox(Bounds bounds, Logger logger)
            : base(bounds, () => getLoggerEntries(logger), formatLoggerEntry)
        {
        }

        private static List<Logger.Entry> getLoggerEntries(Logger logger)
        {
            return logger.GetSafeRecentEntries().Where(entry => visibleSeverities.Contains(entry.Severity)).ToList();
        }

        private static (string, Color) formatLoggerEntry(Logger.Entry entry) => (entry.Text, colors[entry.Severity]);
    }
}
