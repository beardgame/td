using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [Component("sink")]
    sealed class EnemySink<T> : EnemySinkBase<T>
        where T : IComponentOwner, IGameObject, IPositionable
    {
        private Maybe<IHealth> healthComponent;

        protected override void OnAdded()
        {
            base.OnAdded();
            healthComponent = Owner.GetComponents<IHealth>().MaybeSingle();
        }

        protected override void AddSink(Tile t)
        {
            Owner.Game.Navigator.AddSink(t);
        }

        protected override void RemoveSink(Tile t)
        {
            Owner.Game.Navigator.RemoveSink(t);
        }

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
