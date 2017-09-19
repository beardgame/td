using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.Math;
using OpenTK;
using OpenTK.Input;

using static Bearded.TD.Constants.Camera;

namespace Bearded.TD.Game.UI
{
    class MouseCameraController
    {
        private readonly GameCamera camera;
        private readonly float levelRadius;

        private float maxCameraRadius => levelRadius;
        private float maxCameraDistance => levelRadius;
        private float zoomSpeed => BaseZoomSpeed * (1 + camera.Distance * ZoomSpeedFactor);

        private readonly Dictionary<Func<InputState, IAction>, Vector2> scrollActions;
        private readonly Dictionary<Func<InputState, IAction>, float> zoomActions;

        private bool isDragging;
        private float cameraGoalDistance;
        private Vector2 mousePosInWorldSpace;

        public MouseCameraController(GameCamera camera, float levelRadius)
        {
            this.camera = camera;
            this.levelRadius = levelRadius;
            cameraGoalDistance = camera.Distance;

            scrollActions = new Dictionary<Func<InputState, IAction>, Vector2>
            {
                { actionFunc(Key.Left, Key.A), -Vector2.UnitX },
                { actionFunc(Key.Right, Key.D), Vector2.UnitX },
                { actionFunc(Key.Up, Key.W), Vector2.UnitY },
                { actionFunc(Key.Down, Key.S), -Vector2.UnitY },
            };
            zoomActions = new Dictionary<Func<InputState, IAction>, float>
            {
                { actionFunc(Key.PageDown), 1f },
                { actionFunc(Key.PageUp), -1f },
            };
        }

        private static Func<InputState, IAction> actionFunc(params Key[] keys)
        {
            return input => InputAction.AnyOf(keys.Select(input.ForKey));
        }

        public void HandleInput(UpdateEventArgs args, InputState input)
        {
            updateScrolling(args, input);
            updateZoom(args, input);
            updateDragging(input);

            if (!isDragging)
                constrictCameraToLevel(args);
        }
        
        private void updateScrolling(UpdateEventArgs args, InputState input)
        {
            var scrollSpeed = BaseScrollSpeed * camera.Distance;
            var velocity = scrollActions.Aggregate(Vector2.Zero, (v, a) => v + a.Key(input).AnalogAmount * a.Value);
            camera.Position += velocity * scrollSpeed * args.ElapsedTimeInSf;
        }

        private void updateZoom(UpdateEventArgs args, InputState input)
        {
            updateCameraGoalDistance(args, input);
            updateCameraDistance(args, input);
        }

        private void updateCameraGoalDistance(UpdateEventArgs args, InputState input)
        {
            var mouseScroll = -input.DeltaScroll * ScrollTickValue * zoomSpeed;

            var velocity = zoomActions.Aggregate(0f, (v, a) => v + a.Key(input).AnalogAmount * a.Value);

            var newCameraDistance = cameraGoalDistance + mouseScroll + velocity * zoomSpeed * args.ElapsedTimeInSf;

            newCameraDistance = newCameraDistance.Clamped(ZMin * 0.9f, maxCameraDistance * 1.1f);

            float error = 0;

            if (newCameraDistance < ZMin)
            {
                error = newCameraDistance - ZMin;
            }
            else if (newCameraDistance > maxCameraDistance)
            {
                error = newCameraDistance - maxCameraDistance;
            }

            var snapFactor = 1 - Mathf.Pow(1e-8f, args.ElapsedTimeInSf);

            newCameraDistance -= error * snapFactor;

            cameraGoalDistance = newCameraDistance;
        }

        private void updateCameraDistance(UpdateEventArgs args, InputState input)
        {
            var error = camera.Distance - cameraGoalDistance;
            var snapFactor = 1 - Mathf.Pow(1e-6f, args.ElapsedTimeInSf);
            var oldMouseWorldPosition = camera.TransformScreenToWorldPos(input.MousePosition);
            camera.Distance -= error * snapFactor;

            var newMouseWorldPosition = camera.TransformScreenToWorldPos(input.MousePosition);
            var positionError = newMouseWorldPosition - oldMouseWorldPosition;
            camera.Position -= positionError;
        }

        private void updateDragging(InputState input)
        {
            if (isDragging)
                continueDragging(input.MousePosition);
            else if (input.Drag.Active)
                startDragging(input.MousePosition);

            if (!input.Drag.Active && isDragging)
                stopDragging();
        }

        private void startDragging(Vector2 mousePos)
        {
            mousePosInWorldSpace = camera.TransformScreenToWorldPos(mousePos);
            isDragging = true;
        }

        private void continueDragging(Vector2 mousePos)
        {
            var currMousePos = camera.TransformScreenToWorldPos(mousePos);
            var error = currMousePos - mousePosInWorldSpace;
            camera.Position -= error;
        }

        private void stopDragging()
        {
            isDragging = false;
        }

        private void constrictCameraToLevel(UpdateEventArgs args)
        {
            var currentMaxCameraRadiusNormalised = 1 - (camera.Distance / maxCameraDistance).Squared().Clamped(0, 1);
            var currentMaxCameraRadius = maxCameraRadius * currentMaxCameraRadiusNormalised;

            if (camera.Position.LengthSquared <= currentMaxCameraRadius.Squared())
                return;

            var snapBackFactor = 1 - Mathf.Pow(0.01f, args.ElapsedTimeInSf);
            var goalPosition = camera.Position.Normalized() * currentMaxCameraRadius;
            var error = goalPosition - camera.Position;
            camera.Position += error * snapBackFactor;
        }
    }
}
