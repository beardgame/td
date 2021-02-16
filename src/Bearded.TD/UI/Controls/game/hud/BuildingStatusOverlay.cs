using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Utilities;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.UI.Controls
{
    [UsedImplicitly]
    sealed class BuildingStatusOverlay : UpdateableNavigationNode<IPlacedBuilding>
    {
        private GameInstance? game;
        private Pulse pulse = null!;

        public IPlacedBuilding Building { get; private set; } = null!;

        public GameInstance Game => game!;
        public IPulse Pulse => pulse;

        // TODO: invoke this event when the placeholder is replaced by the building instead of closing overlay
        public event VoidEventHandler? BuildingSet;

        protected override void Initialize(DependencyResolver dependencies, IPlacedBuilding building)
        {
            base.Initialize(dependencies, building);
            Building = building;
            Building.Deleting += Close;

            game = dependencies.Resolve<GameInstance>();
            pulse = new Pulse(game.State, Constants.UI.Statistics.TimeBetweenUIUpdates);
        }

        public override void Update(UpdateEventArgs args)
        {
            pulse.Update();
        }

        public override void Terminate()
        {
            Building.Deleting -= Close;

            base.Terminate();
        }

        public void Close()
        {
            Navigation.Exit();
        }
    }
}
