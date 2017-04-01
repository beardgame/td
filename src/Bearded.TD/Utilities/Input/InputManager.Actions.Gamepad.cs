using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities.Input.Actions;
using OpenTK.Input;

namespace Bearded.TD.Utilities.Input
{
    using ButtonSelector = Func<GamePadState, ButtonState>;
    using AxisSelector = Func<GamePadState, float>;

    partial class InputManager
    {
        public partial struct ActionConstructor
        {
            public GamepadsActions Gamepad => new GamepadsActions(manager);
        }

        public struct GamepadsActions
        {
            private readonly InputManager manager;

            public GamepadsActions(InputManager inputManager)
            {
                manager = inputManager;
            }

            public GamepadActions WithId(int id) => new GamepadActions(manager, id);

            public IAction FromString(string name)
            {
                var lower = name.ToLowerInvariant().Trim();
                if (!lower.StartsWith("gamepad"))
                    return null;
                var split = lower.Substring(7).Split(':');
                if (split.Length != 2)
                    throw new ArgumentException("Gamepad button name must have exactly one ':'.", nameof(name));
                int id;
                if (!int.TryParse(split[0].Trim(), out id))
                    throw new ArgumentException("Gamepad button name must include gamepad id.", nameof(name));
                if (id < 0)
                    throw new ArgumentException("Gamepad id must not be negative.", nameof(name));

                return WithId(id).FromButtonName(split[1].Trim());
            }
        }

        public struct GamepadActions
        {
            private readonly InputManager manager;
            private readonly int padId;

            public GamepadActions(InputManager inputManager, int id)
            {
                manager = inputManager;
                padId = id;
            }

            public IAction FromButtonName(string name)
            {
                if (!IsConnected)
                {
                    // this may not be the best solution
                    // but it prevents crashing and things from being overridden, if a gamepad is not connected
                    return new DummyAction(name);
                }

                var action = Buttons.FromName(name) ?? Axes.FromName(name);

                if (action != null)
                    return action;

                throw new ArgumentException("Gamepad button name unknown.", nameof(name));
            }

            public bool IsConnected => padId >= 0 && padId < manager.GamePads.Count
                                       && manager.GamePads[padId].State.Current.IsConnected;

            public GamepadButtonActions Buttons => new GamepadButtonActions(manager, padId);
            public GamepadAxisActions Axes => new GamepadAxisActions(manager, padId);

            public IEnumerable<IAction> All => Buttons.All.Cast<IAction>().Concat(Axes.All);
        }

        public struct GamepadButtonActions
        {
            private readonly InputManager manager;
            private readonly int padId;

            public GamepadButtonActions(InputManager inputManager, int id)
            {
                manager = inputManager;
                padId = id;
            }

            private IAction button(string name)
                => new GamePadButtonAction(manager, padId, name, selectors[name]);

            public IAction FromName(string name)
            {
                ButtonSelector selector;
                return selectors.TryGetValue(name, out selector)
                    ? new GamePadButtonAction(manager, padId, name, selector)
                    : null;
            }

            public IAction A => button("a");
            public IAction B => button("b");
            public IAction X => button("x");
            public IAction Y => button("y");

            public IAction Start => button("start");
            public IAction Back => button("back");
            public IAction Bigbutton => button("bigbutton");

            public IAction LeftShoulder => button("leftshoulder");
            public IAction LeftStick => button("leftstick");
            public IAction RightShoulder => button("rightshoulder");
            public IAction RightStick => button("rightstick");

            public IAction DpadRight => button("dpadright");
            public IAction DpadLeft => button("dpadleft");
            public IAction DpadUp => button("dpadup");
            public IAction DpadDown => button("dpaddown");

            public IEnumerable<GamePadButtonAction> All
            {
                get
                {
                    var inputManager = manager;
                    var id = padId;
                    return selectors.Select(
                        pair => new GamePadButtonAction(inputManager, id, pair.Key, pair.Value)
                    );
                }
            }

            private static readonly Dictionary<string, ButtonSelector> selectors =
                new Dictionary<string, ButtonSelector>
                {
                    {"a", b => b.Buttons.A},
                    {"b", b => b.Buttons.B},
                    {"x", b => b.Buttons.X},
                    {"y", b => b.Buttons.Y},

                    {"start", b => b.Buttons.Start},
                    {"back", b => b.Buttons.Back},
                    {"bigbutton", b => b.Buttons.BigButton},

                    {"leftshoulder", b => b.Buttons.LeftShoulder},
                    {"leftstick", b => b.Buttons.LeftStick},
                    {"rightshoulder", b => b.Buttons.RightShoulder},
                    {"rightstick", b => b.Buttons.RightStick},

                    {"dpadright", b => b.DPad.Right},
                    {"dpadleft", b => b.DPad.Left},
                    {"dpadup", b => b.DPad.Up},
                    {"dpaddown", b => b.DPad.Down}
                };
        }

        public struct GamepadAxisActions
        {
            private readonly InputManager manager;
            private readonly int padId;

            public GamepadAxisActions(InputManager inputManager, int id)
            {
                manager = inputManager;
                padId = id;
            }

            private IAction axis(string name)
                => new GamePadAxisAction(manager, padId, name, selectors[name]);

            public IAction FromName(string name)
            {
                AxisSelector selector;
                return selectors.TryGetValue(name, out selector)
                    ? new GamePadAxisAction(manager, padId, name, selector)
                    : null;
            }

            public IAction XPositive => axis("+x");
            public IAction XNegative => axis("-x");

            public IAction YPositive => axis("+y");
            public IAction YNegative => axis("-y");

            public IAction ZPositive => axis("+z");
            public IAction ZNegative => axis("-z");

            public IAction WPositive => axis("+w");
            public IAction WNegative => axis("-w");

            public IAction TriggerLeft => axis("triggerleft");
            public IAction TriggerRight => axis("triggerright");

            public IEnumerable<GamePadAxisAction> All
            {
                get
                {
                    var inputManager = manager;
                    var id = padId;
                    return selectors.Select(
                        pair => new GamePadAxisAction(inputManager, id, pair.Key, pair.Value)
                    );
                }
            }

            private static readonly Dictionary<string, AxisSelector> selectors =
                new Dictionary<string, AxisSelector>
                {
                    {"+x", s => s.ThumbSticks.Left.X},
                    {"-x", s => -s.ThumbSticks.Left.X},

                    {"+y", s => s.ThumbSticks.Left.Y},
                    {"-y", s => -s.ThumbSticks.Left.Y},

                    {"+z", s => s.ThumbSticks.Right.X},
                    {"-z", s => -s.ThumbSticks.Right.X},

                    {"+w", s => s.ThumbSticks.Right.Y},
                    {"-w", s => -s.ThumbSticks.Right.Y},

                    {"triggerleft", s => s.Triggers.Left},
                    {"triggerright", s => s.Triggers.Right}
                };
        }

    }
}