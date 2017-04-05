﻿using System.Collections.Generic;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.Math;
using OpenTK;
using OpenTK.Input;
using static Bearded.TD.Constants.Camera;

namespace Bearded.TD.Game
{
    class GameCamera
    {
        private readonly Dictionary<IAction, Vector2> scrollActions;

        private readonly Dictionary<IAction, float> zoomActions;

        private readonly InputManager inputManager;
        private readonly GameMeta meta;
        private readonly float levelRadius;

        private Vector2? mousePosInWorldSpace;
        private ViewportSize viewportSize;

        private Vector2 cameraPosition;
        private float cameraDistance;
        private float zoomSpeed => BaseZoomSpeed * (1 + cameraDistance * ZoomSpeedFactor);

        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ScreenToWorld { get; private set; }

        public GameCamera(InputManager inputManager, GameMeta meta, float levelRadius)
        {
            this.inputManager = inputManager;
            this.meta = meta;
            this.levelRadius = levelRadius;

            cameraPosition = Vector2.Zero;
            cameraDistance = ZDefault;
            recalculateViewMatrix();

            scrollActions =  new Dictionary<IAction, Vector2>
            {
                {keyOrAxis(Key.Left,"-x"), -Vector2.UnitX},
                {keyOrAxis(Key.Right, "+x"), Vector2.UnitX},
                {keyOrAxis(Key.Up,"+y"), Vector2.UnitY},
                {keyOrAxis(Key.Down,"-y"), -Vector2.UnitY},
            };

            zoomActions = new Dictionary<IAction, float>
            {
                {keyOrAxis(Key.PageDown,":+z"), 1f},
                {keyOrAxis(Key.PageUp,"-z"), -1f},
            };
        }

        private IAction keyOrAxis(Key key, string axis)
            => inputManager.Actions.Keyboard.FromKey(key).Or(inputManager.Actions.Gamepad.WithId(0).FromButtonName(axis));

        public void OnViewportSizeChanged(ViewportSize viewportSize)
        {
            this.viewportSize = viewportSize;
        }

        public void Update(float elapsedTime)
        {
            if (inputManager.RightMousePressed || mousePosInWorldSpace != null)
                updateDragging();
            if (!inputManager.RightMousePressed)
                updateScrolling(elapsedTime);

            recalculateViewMatrix();
        }

        private void updateDragging()
        {
            if (inputManager.RightMousePressed && !mousePosInWorldSpace.HasValue)
            {
                mousePosInWorldSpace = getMouseWorldPosition();
                meta.Logger.Trace.Log("Start drag at {0}", mousePosInWorldSpace);
            } else if (mousePosInWorldSpace.HasValue)
            {
                var currMousePos = getMouseWorldPosition();
                var error = currMousePos - mousePosInWorldSpace.Value;
                cameraPosition -= error;
            }

            if (inputManager.RightMouseReleased && mousePosInWorldSpace != null)
            {
                mousePosInWorldSpace = null;
                meta.Logger.Trace.Log("End drag");
            }
        }

        private void updateScrolling(float elapsedTime)
        {
            cameraDistance -= inputManager.DeltaScroll * ScrollTickValue * zoomSpeed;
            foreach (var zoomAction in zoomActions)
            {
                cameraDistance += zoomAction.Key.AnalogAmount * elapsedTime
                                  * zoomSpeed * zoomAction.Value;
            }
            cameraDistance = cameraDistance.Clamped(ZMin, levelRadius);

            var scrollSpeed = BaseScrollSpeed * cameraDistance;

            foreach (var scrollAction in scrollActions)
            {
                cameraPosition += scrollAction.Key.AnalogAmount * elapsedTime
                                  * scrollSpeed * scrollAction.Value;
            }

            var maxDistanceFromOrigin = levelRadius - cameraDistance;
            if (cameraPosition.LengthSquared > maxDistanceFromOrigin.Squared())
            {
                cameraPosition = cameraPosition.Normalized() * maxDistanceFromOrigin;
            }
        }

        private void recalculateViewMatrix()
        {
            var eye = cameraPosition.WithZ(cameraDistance);
            var target = cameraPosition.WithZ();
            ViewMatrix = Matrix4.LookAt(eye, target, Vector3.UnitY);
        }

        private Vector2 getMouseWorldPosition()
        {
            return TransformScreenToWorldPos(inputManager.MousePosition);
        }

        public Vector2 TransformScreenToWorldPos(Vector2 screenPos)
        {
            // This is simple right now under the assumptions:
            // * The camera always looks straight down. That is, the camera eye and target both lie
            //   along the infinite extension of cameraPosition in the Z axis.
            // * The FoV is Pi/2
            return cameraPosition + cameraDistance * getNormalisedScreenPosition(screenPos);
        }

        private Vector2 getNormalisedScreenPosition(Vector2 screenPos)
        {
            var ret = new Vector2(
                2 * screenPos.X / viewportSize.Width - 1,
                1 - 2 * screenPos.Y / viewportSize.Height
            );
            ret.X *= viewportSize.AspectRatio;
            return ret;
        }
    }
}
