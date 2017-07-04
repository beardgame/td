using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Meta;
using Bearded.Utilities;

namespace Bearded.TD.UI.Components
{
    class ConsoleTextBox : TextBox<Logger.Entry>
    {
        private readonly List<Logger.Entry> entries = new List<Logger.Entry>(); 
        private readonly Logger logger;

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
        private static Logger.Severity lowestVisibleSeverity() =>
            UserSettings.Instance.Misc.ShowTraceMessages ? Logger.Severity.Trace : Logger.Severity.Debug;
#else
        private static Logger.Severity lowestVisibleSeverity() => Logger.Severity.Info;
#endif

        public ConsoleTextBox(Bounds bounds, Logger logger)
            : base(bounds)
        {
            this.logger = logger;
        }
        
        protected override IReadOnlyList<Logger.Entry> GetItems()
        {
            entries.Clear();
            logger.CopyRecentEntriesWithSeverity(lowestVisibleSeverity(), entries);
            return entries;
        }

        protected override (string, Color) Format(Logger.Entry item)
            => (item.Text, colors[item.Severity]);
    }
}
