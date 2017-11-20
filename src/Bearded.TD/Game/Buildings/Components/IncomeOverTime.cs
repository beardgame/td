using Bearded.TD.Game.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings.Components
{
    class IncomeOverTime : Component
    {
        private const float incomePerSecond = 5;

        protected override void Initialise() { }

        public override void Update(TimeSpan elapsedTime)
        {
            Building.Faction.Resources.ProvideResourcesOverTime(incomePerSecond);
        }

        public override void Draw(GeometryManager geometries) { }
    }
}
