using System.Collections.Generic;
using Bearded.Utilities.Input;
using Bearded.Utilities.Math;
using OpenTK;
using OpenTK.Input;
using static Bearded.TD.Constants.Camera;

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

        private static readonly Dictionary<IAction, float> zoomActions = new Dictionary<IAction, float>
        {
            {KeyboardAction.FromKey(Key.PageDown).Or(GamePadAction.FromString("gamepad0:+z")), 1f},
            {KeyboardAction.FromKey(Key.PageUp).Or(GamePadAction.FromString("gamepad0:-z")), -1f},
        };

        private readonly GameMeta meta;
        private readonly float levelRadius;

        public Vector2 CameraPosition { get; private set; }
        public float CameraDistance { get; private set; }

        public GameCamera(GameMeta meta, float levelRadius)
        {
            this.meta = meta;
            this.levelRadius = levelRadius;

            CameraPosition = Vector2.Zero;
            CameraDistance = ZDefault;
        }

        public void Update(float elapsedTime)
        {
            foreach (var zoomAction in zoomActions)
            {
                CameraDistance += zoomAction.Key.AnalogAmount * elapsedTime
                                  * ZoomSpeed * zoomAction.Value;
            }
            CameraDistance = CameraDistance.Clamped(ZMin, levelRadius);

            var scrollSpeed = BaseScrollSpeed * CameraDistance;

            foreach (var scrollAction in scrollActions)
            {
                CameraPosition += scrollAction.Key.AnalogAmount * elapsedTime
                                  * scrollSpeed * scrollAction.Value;
            }

            var maxDistanceFromOrigin = levelRadius - CameraDistance;
            if (CameraPosition.LengthSquared > maxDistanceFromOrigin.Squared())
            {
                CameraPosition = CameraPosition.Normalized() * maxDistanceFromOrigin;
            }
        }
    }
}
