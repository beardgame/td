using Bearded.TD.Game;
using Bearded.TD.Game.Camera;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Controls
{
    sealed class ManualControlReportControl : ReportControl
    {
        private readonly GameInstance game;
        private readonly IManualControlReport report;

        public override double Height { get; }

        public ManualControlReportControl(GameInstance game, IManualControlReport report)
        {
            this.game = game;
            this.report = report;

            var column = this.BuildFixedColumn();
            column.AddButton(b => b.WithLabel("Assume Direct Control").WithOnClick(startControl));
            Height = column.Height;
        }

        private void startControl()
        {
            game.SelectionManager.ResetSelection();
            game.PlayerInput.SetInteractionHandler(new ControlOverride(game, report));
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        private class ControlOverride : InteractionHandler, IManualTarget2
        {
            private readonly IManualControlReport report;

            public Position2 Target { get; private set; }
            public bool TriggerDown { get; private set; }

            public ControlOverride(GameInstance game, IManualControlReport report) : base(game)
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
