using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Meta;
using Bearded.TD.Utilities.Console;
using Bearded.UI.Navigation;
using Bearded.Utilities.IO;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    class GameDebugOverlay : NavigationNode<Void>
    {
        private Logger logger;
        private readonly List<Item> items = new List<Item>();

        public ReadOnlyCollection<Item> Items { get; }

        public GameDebugOverlay()
        {
            Items = items.AsReadOnly();
        }

        public class Item
        {
        }

        public sealed class Command : Item
        {
            private readonly Logger logger;
            private readonly string command;

            public string Name => command;

            public Command(string command, Logger logger)
            {
                this.logger = logger;
                this.command = command;
            }

            public void Call()
            {
                var splitCommand = command.Split(' ');
                ConsoleCommands.TryRun(splitCommand[0], logger,
                    new CommandParameters(splitCommand.Skip(1).ToArray()));
            }
        }

        protected override void Initialize(DependencyResolver dependencies, Void parameters)
        {
            logger = dependencies.Resolve<Logger>();

            resetItems();
        }

        private void resetItems()
        {
            items.Clear();

            addCommands(
                "game.techpoints 1000",
                "game.resources 1000",
                "game.killall",
                "game.repairall",
                "game.die"
                );

            // extract debug settings from UserSettings

            void addCommands(params string[] commands)
            {
                foreach (var command in commands)
                {
                    items.Add(new Command(command, logger));
                }
            }
        }

        public void Close()
        {
            UserSettings.Instance.Debug.GameDebugScreen = false;
            UserSettings.Save(logger);
        }
    }
}
