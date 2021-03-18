using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game.Generation;
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
    sealed class GameDebugOverlay : NavigationNode<Void>
    {
        private readonly LocalBool randomiseTilemapGenerationSeed = new("randomise seed");
        private Logger logger;
        private readonly List<Item> items = new();

        public ReadOnlyCollection<Item> Items { get; }

        public event VoidEventHandler? ItemsChanged;

        public GameDebugOverlay()
        {
            Items = items.AsReadOnly();
        }

        protected override void Initialize(DependencyResolver dependencies, Void parameters)
        {
            logger = dependencies.Resolve<Logger>();
            randomiseTilemapGenerationSeed.ValueChanged += onItemsChanged;

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

            items.Add(new Header("LEVEL GENERATION"));

            items.Add(randomiseTilemapGenerationSeed);

            items.AddRange(
                Enum.GetNames<LevelGenerationMethod>()
                    .Select(m => new ParameterisedCommand($"game.generateterrain {m}", logger,
                        () => randomiseTilemapGenerationSeed.Value ? "random" : ""))
                    .ToArray()
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

        private void onItemsChanged()
        {
            ItemsChanged?.Invoke();
        }

        public abstract class Item
        {
            protected void RunCommand(string command, Logger logger)
            {
                logger.Debug!.Log($"Debug UI runs: {command}");
                var splitCommand = command.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                ConsoleCommands.TryRun(splitCommand[0], logger,
                    new CommandParameters(splitCommand.Skip(1).ToImmutableArray()));
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

        public class Command : Item
        {
            protected Logger Logger { get; }
            protected string CommandName { get; }

            public string Name => CommandName;

            public Command(string command, Logger logger)
            {
                Logger = logger;
                CommandName = command;
            }

            public virtual void Call()
            {
                RunCommand(CommandName, Logger);
            }
        }

        public sealed class ParameterisedCommand : Command
        {
            private readonly Func<string> getParameters;

            public ParameterisedCommand(string command, Logger logger, Func<string> getParameters)
                : base(command, logger)
            {
                this.getParameters = getParameters;
            }

            public override void Call()
            {
                RunCommand($"{CommandName} {getParameters()}", Logger);
            }
        }

        public abstract class Setting<T> : Item
        {
            private readonly Logger logger;
            private readonly string setting;

            public string Name => setting;
            public T Value { get; }

            protected Setting(string setting, Logger logger)
            {
                this.setting = setting;
                this.logger = logger;

                Value = (T)
                    typeof(UserSettings.DebugSettings)
                        .GetField(setting)
                        ?.GetValue(UserSettings.Instance.Debug);
            }

            protected void Set(T value)
            {
                RunCommand($"setting debug.{setting} {value?.ToString()?.ToLower()}", logger);
            }
        }

        sealed class BoolSetting : Setting<bool>, IBoolSetting
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
        sealed class LocalBool : Item, IBoolSetting
        {
            public string Name { get; }
            public bool Value { get; private set; }

            public event VoidEventHandler ValueChanged;

            public LocalBool(string name)
            {
                Name = name;
            }

            public void Toggle()
            {
                Value = !Value;
                ValueChanged?.Invoke();
            }
        }

        public interface IBoolSetting
        {
            string Name { get; }
            bool Value { get; }
            void Toggle();
        }
    }
}
