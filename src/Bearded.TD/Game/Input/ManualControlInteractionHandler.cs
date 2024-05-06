using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Camera;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.Game.Input;

sealed class ManualControlInteractionHandler : InteractionHandler, IManualTarget2
{
    private readonly IManualControl manualControl;

    public Position2 Target { get; private set; }
    public bool TriggerDown { get; private set; }

    public ManualControlInteractionHandler(GameInstance game, IManualControl manualControl) : base(game)
    {
        this.manualControl = manualControl;
    }

    protected override void OnStart(ICursorHandler cursor)
    {
        manualControl.StartControl(this, cancelControl);
        cursor.SetCameraController(new CameraController(Game.CameraController, manualControl, this));
    }

    private void cancelControl()
    {
        Game.PlayerInput.ResetInteractionHandler();
    }

    protected override void OnEnd(ICursorHandler cursor)
    {
        cursor.ResetCameraController();
        manualControl.EndControl();
    }

    public override void Update(ICursorHandler cursor)
    {
        Target = cursor.CursorPosition;
        TriggerDown = cursor.Click.Active;

        if (cursor.Cancel.Hit)
        {
            Game.PlayerInput.ResetInteractionHandler();
        }
    }

    private sealed class CameraController : ICameraController
    {
        private readonly GameCameraController controller;
        private readonly IManualControl manualControl;
        private readonly IManualTarget2 target;

        private readonly Dictionary<Func<InputState, ActionState>, float> zoomActions;

        public CameraController(GameCameraController controller, IManualControl manualControl, IManualTarget2 target)
        {
            this.controller = controller;
            this.manualControl = manualControl;
            this.target = target;

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
            updateZoom(input);
            centerOnPointBetweenSubjectAndTarget();
        }

        private void updateZoom(InputState input)
        {
            controller.OverrideMaxCameraDistance(manualControl.SubjectRange.NumericValue);
            controller.CenterZoomAnchor();

            var mouseScroll = -input.Mouse.DeltaScroll * Constants.Camera.ScrollTickValue;
            controller.ConstantZoom(mouseScroll);

            var velocity = zoomActions.Aggregate(0f, (v, a) => v + a.Key(input).AnalogAmount * a.Value);
            controller.Zoom(velocity);
        }

        private void centerOnPointBetweenSubjectAndTarget()
        {
            var subjectP = manualControl.SubjectPosition;
            var targetP = target.Target;
            var pointBetweenSubjectAndTarget = subjectP + (targetP - subjectP) * 0.5f;
            controller.ScrollToWorldPos(pointBetweenSubjectAndTarget);
        }

        public void Stop()
        {
            controller.ResetMaxCameraDistanceOverride();
        }
    }
}
