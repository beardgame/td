using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Meta;
using Bearded.TD.Utilities.Console;
using Bearded.UI.Controls;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    class GameDebugOverlay : NavigationNode<Void>
    {
        private Logger logger;
        private readonly List<Item> items = new List<Item>();

        public ReadOnlyCollection<Item> Items { get; }

        public event VoidEventHandler? ItemsChanged;

        public GameDebugOverlay()
        {
            Items = items.AsReadOnly();
        }

        protected override void Initialize(DependencyResolver dependencies, Void parameters)
        {
            logger = dependencies.Resolve<Logger>();

            UserSettings.SettingsChanged += resetItems;

            resetItems();
        }

        private void resetItems()
        {
            items.Clear();

            items.Add(new Header("COMMANDS"));

            addCommands(
                "game.techpoints 1000",
                "game.resources 1000",
                "game.killall",
                "game.repairall",
                "game.die",
                "debug.ui"
            );

            items.Add(new Header("SETTINGS"));

            items.AddRange(typeof(UserSettings.DebugSettings).GetFields().Select(
                    field =>
                    {
                        if (field.Name == nameof(UserSettings.DebugSettings.GameDebugScreen))
                            return (Item)null;

                        if (field.FieldType == typeof(bool))
                            return new BoolSetting(field.Name, logger);

                        if (field.GetCustomAttributes(typeof(SettingOptionsAttribute), false).FirstOrDefault()
                            is SettingOptionsAttribute attribute)
                            return new OptionsSetting(field.Name, attribute.Options, logger);

                        return null;
                    }
                ).NotNull()
            );


            ItemsChanged?.Invoke();

            void addCommands(params string[] commands)
            {
                items.AddRange(commands.Select(c => new Command(c, logger)));
            }
        }

        public void Close(Button.ClickEventArgs _)
        {
            UserSettings.Instance.Debug.GameDebugScreen = false;
            UserSettings.Save(logger);
        }

        public override void Terminate()
        {
            base.Terminate();

            UserSettings.SettingsChanged -= resetItems;
        }

        public abstract class Item
        {
            protected void RunCommand(string command, Logger logger)
            {
                logger.Debug.Log($"Debug UI runs: {command}");
                var splitCommand = command.Split(' ');
                ConsoleCommands.TryRun(splitCommand[0], logger,
                    new CommandParameters(splitCommand.Skip(1).ToArray()));
            }
        }

        public sealed class Header : Item
        {
            public string Name { get; }

            public Header(string name)
            {
                Name = name;
            }
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
                RunCommand(command, logger);
            }
        }

        public abstract class Setting<T> : Item
        {
            private readonly Logger logger;
            private readonly string setting;

            public string Name => setting;
            public T Value { get; }

            public Setting(string setting, Logger logger)
            {
                this.setting = setting;
                this.logger = logger;

                Value = (T)
                    typeof(UserSettings.DebugSettings)
                        .GetField(setting)
                        .GetValue(UserSettings.Instance.Debug);
            }

            protected void Set(T value)
            {
                RunCommand($"setting debug.{setting} {value.ToString().ToLower()}", logger);
            }
        }

        public sealed class BoolSetting : Setting<bool>
        {
            public BoolSetting(string setting, Logger logger) : base(setting, logger)
            {
            }

            public void Toggle()
            {
                Set(!Value);
            }
        }

        public sealed class OptionsSetting : Setting<object>
        {
            public ImmutableArray<object> Options { get; }

            public OptionsSetting(string setting, object[] options, Logger logger) : base(setting, logger)
            {
                Options = ImmutableArray.Create(options);
            }

            public void Set(int index)
            {
                Set(Options[index]);
            }
        }
    }
}
