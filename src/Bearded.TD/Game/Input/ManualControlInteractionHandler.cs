using Bearded.TD.Game.Camera;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input
{
    sealed class ManualControlInteractionHandler : InteractionHandler, IManualTarget2
    {
        private readonly IManualControlReport report;

        public Position2 Target { get; private set; }
        public bool TriggerDown { get; private set; }

        public ManualControlInteractionHandler(GameInstance game, IManualControlReport report) : base(game)
        {
            this.report = report;
        }

        protected override void OnStart(ICursorHandler cursor)
        {
            report.StartControl(this);
            cursor.SetCameraController(new CameraController(Game.CameraController, report, this));
        }

        protected override void OnEnd(ICursorHandler cursor)
        {
            cursor.ResetCameraController();
            report.EndControl();
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

        private class CameraController : ICameraController
        {
            private readonly GameCameraController controller;
            private readonly IManualControlReport report;
            private readonly IManualTarget2 target;

            public CameraController(GameCameraController controller, IManualControlReport report, IManualTarget2 target)
            {
                this.controller = controller;
                this.report = report;
                this.target = target;
            }

            public void HandleInput(InputState input)
            {
                var subjectP = report.SubjectPosition;
                var targetP = target.Target;
                var cameraP = subjectP + (targetP - subjectP) * 0.5f;
                controller.ScrollToWorldPos(cameraP);
            }

            public void Stop()
            {
            }
        }
    }
}
