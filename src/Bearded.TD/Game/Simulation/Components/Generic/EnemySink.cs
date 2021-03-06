﻿using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components.Damage;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Generic
{
    [Component("sink")]
    sealed class EnemySink<T> : Component<T>
        where T : IGameObject, IPlacedBuilding, IPositionable
    {
        private Maybe<IHealth> healthComponent;

        protected override void Initialize()
        {
            foreach (var tile in Owner.OccupiedTiles)
            {
                Owner.Game.Navigator.AddSink(tile);
            }

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
