using System.Collections.Generic;
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
        private static readonly Logger.Severity lowestVisibleSeverity = Logger.Severity.Trace;
#else
        private static readonly Logger.Severity lowestVisibleSeverity = Logger.Severity.Info;
#endif

        public ConsoleTextBox(Bounds bounds, Logger logger)
            : base(bounds, () => getLoggerEntries(logger), formatLoggerEntry)
        {
        }

        private static IReadOnlyList<Logger.Entry> getLoggerEntries(Logger logger)
            => logger.GetSafeRecentEntriesWithSeverity(lowestVisibleSeverity);

        private static (string, Color) formatLoggerEntry(Logger.Entry entry)
            => (entry.Text, colors[entry.Severity]);
    }
}
