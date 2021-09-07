using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [Component("sink")]
    sealed class EnemySink<T> : Component<T>
        where T : IComponentOwner, IGameObject, IPositionable
    {
        private Maybe<IHealth> healthComponent;
        private readonly OccupiedTilesTracker occupiedTilesTracker = new();

        protected override void Initialize()
        {
            occupiedTilesTracker.Initialize(Owner, Events);

            foreach (var tile in occupiedTilesTracker.OccupiedTiles)
            {
                Owner.Game.Navigator.AddSink(tile);
            }

            occupiedTilesTracker.TileAdded += Owner.Game.Navigator.AddSink;
            occupiedTilesTracker.TileRemoved += Owner.Game.Navigator.RemoveSink;

            healthComponent = Owner.GetComponents<IHealth>().MaybeSingle();
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers drawers)
        {
            healthComponent.Match(health =>
            {
                drawers.InGameConsoleFont.DrawLine(
                    xyz: Owner.Position.NumericValue,
                    text: health.CurrentHealth.NumericValue.ToString(),
                    fontHeight: 1,
                    alignHorizontal: .5f,
                    alignVertical: .5f,
                    parameters: Color.White
                );
            });
        }
    }
}
