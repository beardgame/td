using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Camera;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.Game.Input
{
    sealed class MouseCameraController
    {
        private readonly GameCameraController cameraController;

        private readonly Dictionary<Func<InputState, ActionState>, Difference2> scrollActions;
        private readonly Dictionary<Func<InputState, ActionState>, float> zoomActions;

        private bool isDragging;

        public MouseCameraController(GameCameraController cameraController)
        {
            this.cameraController = cameraController;

            scrollActions = new Dictionary<Func<InputState, ActionState>, Difference2>
            {
                { actionFunc(Keys.Left, Keys.A), new Difference2(-Vector2.UnitX) },
                { actionFunc(Keys.Right, Keys.D), new Difference2(Vector2.UnitX) },
                { actionFunc(Keys.Up, Keys.W), new Difference2(Vector2.UnitY) },
                { actionFunc(Keys.Down, Keys.S), new Difference2(-Vector2.UnitY) },
            };
            zoomActions = new Dictionary<Func<InputState, ActionState>, float>
            {
                { actionFunc(Keys.PageDown), 1f },
                { actionFunc(Keys.PageUp), -1f },
            };
        }

        private static Func<InputState, ActionState> actionFunc(params Keys[] keys)
        {
            return input => input.ForAnyKey(keys);
        }

        public void HandleInput(InputState input)
        {
            updateScrolling(input);
            updateZoom(input);
            updateDragging(input);
        }

        private void updateScrolling(InputState input)
        {
            var velocity = scrollActions.Aggregate(Difference2.Zero, (v, a) => v + a.Key(input).AnalogAmount * a.Value);
            cameraController.Scroll(velocity);
        }

        private void updateZoom(InputState input)
        {
            cameraController.SetZoomAnchor(input.Mouse.Position);

            var mouseScroll = -input.Mouse.DeltaScroll * Constants.Camera.ScrollTickValue;
            cameraController.ConstantZoom(mouseScroll);

            var velocity = zoomActions.Aggregate(0f, (v, a) => v + a.Key(input).AnalogAmount * a.Value);
            cameraController.Zoom(velocity);
        }

        private void updateDragging(InputState input)
        {
            if (isDragging)
                continueDragging(input.Mouse.Position);
            else if (input.Mouse.Drag.Active)
                startDragging(input.Mouse.Position);

            if (!input.Mouse.Drag.Active && isDragging)
                stopDragging();
        }

        private void startDragging(Vector2 mousePos)
        {
            cameraController.SetScrollAnchor(mousePos);
            cameraController.Grab();
            isDragging = true;
        }

        private void continueDragging(Vector2 mousePos)
        {
            cameraController.MoveScrollAnchor(mousePos);
        }

        private void stopDragging()
        {
            isDragging = false;
            cameraController.Release();
        }
    }
}
