using System.Collections.Generic;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Game
{
    class GameCamera
    {
        private static readonly Dictionary<IAction, Vector2> scrollActions = new Dictionary<IAction, Vector2>
        {
            {KeyboardAction.FromKey(Key.Left).Or(GamePadAction.FromString("gamepad0:-x")), -Vector2.UnitX},
            {KeyboardAction.FromKey(Key.Right).Or(GamePadAction.FromString("gamepad0:+x")), Vector2.UnitX},
            {KeyboardAction.FromKey(Key.Up).Or(GamePadAction.FromString("gamepad0:+y")), Vector2.UnitY},
            {KeyboardAction.FromKey(Key.Down).Or(GamePadAction.FromString("gamepad0:-y")), -Vector2.UnitY},
        };

        private readonly GameMeta meta;
        private readonly float tilemapRadius;

        public Vector2 CameraPosition { get; private set; }
        public float CameraDistance { get; private set; }

        public GameCamera(GameMeta meta, float tilemapRadius)
        {
            this.meta = meta;
            this.tilemapRadius = tilemapRadius;

            CameraPosition = Vector2.Zero;
            CameraDistance = 10;
        }

        public void Update(float elapsedTime)
        {
            foreach (KeyValuePair<IAction, Vector2> scrollAction in scrollActions)
            {
                CameraPosition += scrollAction.Key.AnalogAmount * elapsedTime
                                  * Constants.Camera.ScrollSpeed * scrollAction.Value;
            }
        }
    }
}
