using Bearded.TD.Game.UI.Components;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.Linq;
using OpenTK;
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

        public DebugScreenLayer(
            ScreenLayerCollection parent, GeometryManager geometries, GameInstance game, InputManager inputManager)
            : base(parent, geometries)
        {
            this.game = game;
            rotateAction = inputManager.Actions.Keyboard.FromKey(Key.F3);
            debugComponents = createComponents();
            debugComponents.NotNull().ForEach(AddComponent);

            if (currentOpenComponent < 0 || currentOpenComponent >= debugComponents.Length)
                currentOpenComponent = 0;
            debugComponents[currentOpenComponent]?.Focus();
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
            var bounds = Bounds.AnchoredBox(Screen, BoundsAnchor.End, BoundsAnchor.End, new Vector2(240, 300));
            
            return new FocusableUIComponent[]
            {
                null,
                new DebugGenerationInfo(bounds, game), 
            };
        }
    }
}
