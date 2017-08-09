using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.Linq;
using Bearded.Utilities.Math;
using OpenTK.Input;

namespace Bearded.TD.Game.UI
{
    class DebugScreenLayer : UIScreenLayer
    {
        private readonly GameInstance game;
        private readonly IAction rotateAction;
        private readonly FocusableUIComponent[] debugComponents;

        private int currentOpenComponent
        {
            get => UserSettings.Instance.Debug.InfoScreen;
            set => UserSettings.Instance.Debug.InfoScreen = value;
        } 

        public DebugScreenLayer(ScreenLayerCollection parent, GeometryManager geometries, GameInstance game, InputManager inputManager) : base(parent, geometries)
        {
            this.game = game;
            rotateAction = inputManager.Actions.Keyboard.FromKey(Key.F3);
            debugComponents = createComponents();
            debugComponents.NotNull().ForEach(AddComponent);

            if (currentOpenComponent < 0 || currentOpenComponent >= debugComponents.Length)
                currentOpenComponent = 0;
        }

        protected override bool DoHandleInput(InputContext input)
        {
            if (rotateAction.Hit)
                rotateModes();
            return base.DoHandleInput(input);
        }

        private void rotateModes()
        {
            debugComponents[currentOpenComponent]?.Unfocus();
            currentOpenComponent = (currentOpenComponent + 1) % debugComponents.Length;
            debugComponents[currentOpenComponent]?.Focus();
        }

        private FocusableUIComponent[] createComponents()
        {
            return new FocusableUIComponent[]
            {
                null
            };
        }
    }
}
