using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Utilities.Input
{
    partial class InputManager
    {
        public ActionConstructor Actions => new ActionConstructor(this);

        public partial struct ActionConstructor
        {
            private readonly InputManager manager;
            public ActionConstructor(InputManager inputManager) { manager = inputManager; }

            public IAction None => InputAction.Unbound;

            public IAction FromString(string value)
                => value.ToLowerInvariant().Trim() == "unbound"
                    ? InputAction.Unbound
                    : Keyboard.FromString(value) ?? Gamepad.FromString(value);

            public IEnumerable<IAction> GetAllAvailable()
            {
                var pad = Gamepad;
                return Keyboard.All.Concat(manager.GamePads.SelectMany((_, i) => pad.WithId(i).All));
            }

        }
    }
}