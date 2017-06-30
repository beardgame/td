using System;
using System.Collections.Generic;
using System.Linq;
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

        private float maxCameraRadius => levelRadius;
        private float maxCameraDistance => levelRadius;

        private bool isDragging;
        private Vector2 mousePosInWorldSpace;
        private ViewportSize viewportSize;

        private Vector2 cameraPosition;
        private float cameraDistance;
        private float zoomSpeed => BaseZoomSpeed * (1 + cameraDistance * ZoomSpeedFactor);

        public Matrix4 ViewMatrix { get; private set; }

        public GameCamera(InputManager inputManager, GameMeta meta, float levelRadius)
        {
            this.inputManager = inputManager;
            this.meta = meta;
            this.levelRadius = levelRadius;

            cameraPosition = Vector2.Zero;
            cameraDistance = ZDefault;
            recalculateViewMatrix();

            scrollActions = new Dictionary<IAction, Vector2>
            {
                { axisOrKeys("-x", Key.Left, Key.A), -Vector2.UnitX },
                { axisOrKeys("+x", Key.Right, Key.D), Vector2.UnitX },
                { axisOrKeys("+y", Key.Up, Key.W), Vector2.UnitY },
                { axisOrKeys("-y", Key.Down, Key.S), -Vector2.UnitY },
            };

            zoomActions = new Dictionary<IAction, float>
            {
                { axisOrKeys("+z", Key.PageDown), 1f },
                { axisOrKeys("-z", Key.PageUp), -1f },
            };
        }

        private IAction axisOrKeys(string axis, params Key[] keys)
        {
            return inputManager.Actions.Gamepad.WithId(0).FromButtonName(axis)
                    .Or(InputAction.AnyOf(keys.Select(k => inputManager.Actions.Keyboard.FromKey(k)).ToArray()));
        }

        public void OnViewportSizeChanged(ViewportSize viewportSize)
        {
            this.viewportSize = viewportSize;
        }

        public void Update(float elapsedTime)
        {
            updateDragging();
            updateScrolling(elapsedTime);
            updateZoom(elapsedTime);

            if (!isDragging)
                constrictCameraToLevel(elapsedTime);

            recalculateViewMatrix();
        }

        private void updateDragging()
        {
            if (isDragging)
                continueDragging();
            else if (inputManager.RightMousePressed)
                startDragging();

            if (inputManager.RightMouseReleased && isDragging)
                stopDragging();
        }

        private void startDragging()
        {
            mousePosInWorldSpace = getMouseWorldPosition();
            isDragging = true;

            meta.Logger.Trace.Log("Start drag at {0}", mousePosInWorldSpace);
        }

        private void continueDragging()
        {
            var currMousePos = getMouseWorldPosition();
            var error = currMousePos - mousePosInWorldSpace;
            cameraPosition -= error;
        }

        private void stopDragging()
        {
            isDragging = false;

            meta.Logger.Trace.Log("End drag");
        }

        private void constrictCameraToLevel(float elapsedTime)
        {
            var currentMaxCameraRadiusNormalised = 1 - (cameraDistance / maxCameraDistance).Squared().Clamped(0, 1);
            var currentMaxCameraRadius = maxCameraRadius * currentMaxCameraRadiusNormalised;

            if (cameraPosition.LengthSquared <= currentMaxCameraRadius.Squared())
                return;

            var snapBackFactor = 1 - Mathf.Pow(0.01f, elapsedTime);

            var goalPosition = cameraPosition.Normalized() * currentMaxCameraRadius;

            var error = goalPosition - cameraPosition;

            cameraPosition += error * snapBackFactor;
        }

        private void updateScrolling(float elapsedTime)
        {
            var scrollSpeed = BaseScrollSpeed * cameraDistance;

            foreach (var scrollAction in scrollActions)
            {
                cameraPosition += scrollAction.Key.AnalogAmount * elapsedTime
                                  * scrollSpeed * scrollAction.Value;
            }
        }

        private void updateZoom(float elapsedTime)
        {
            cameraDistance -= inputManager.DeltaScroll * ScrollTickValue * zoomSpeed;
            foreach (var zoomAction in zoomActions)
            {
                cameraDistance += zoomAction.Key.AnalogAmount * elapsedTime
                                  * zoomSpeed * zoomAction.Value;
            }
            cameraDistance = cameraDistance.Clamped(ZMin, maxCameraDistance);
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
