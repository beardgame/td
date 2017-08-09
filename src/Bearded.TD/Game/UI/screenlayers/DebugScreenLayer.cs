using Bearded.TD.Game.UI.Components;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.Linq;
using Bearded.Utilities.Math;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Game.UI
{
    class DebugScreenLayer : UIScreenLayer
    {
        private readonly GameInstance game;
        private readonly IAction rotateAction;
        private readonly FocusableUIComponent[] debugComponents;

        private int currentOpenComponent;

        public DebugScreenLayer(
            ScreenLayerCollection parent, GeometryManager geometries, GameInstance game, InputManager inputManager)
            : base(parent, geometries)
        {
            this.game = game;
            rotateAction = inputManager.Actions.Keyboard.FromKey(Key.F3);
            debugComponents = createComponents();
            debugComponents.NotNull().ForEach(AddComponent);

            currentOpenComponent = UserSettings.Instance.Debug.InfoScreen.Clamped(0, debugComponents.Length - 1);
            debugComponents[currentOpenComponent]?.Focus();

            UserSettings.SettingsChanged += onSettingsChanged;
        }

        private void onSettingsChanged()
        {
            debugComponents[currentOpenComponent]?.Unfocus();
            currentOpenComponent = UserSettings.Instance.Debug.InfoScreen.Clamped(0, debugComponents.Length - 1);
            debugComponents[currentOpenComponent]?.Focus();
        }

        protected override bool DoHandleInput(InputContext input)
        {
            if (rotateAction.Hit)
            {
                UserSettings.Instance.Debug.InfoScreen = (currentOpenComponent + 1) % debugComponents.Length;
                UserSettings.RaiseSettingsChanged();
            }
            return base.DoHandleInput(input);
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
