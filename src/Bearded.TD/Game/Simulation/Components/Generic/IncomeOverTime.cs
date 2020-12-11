using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Generic
{
    [Component("incomeOverTime")]
    class IncomeOverTime : Component<Building, IIncomeOverTimeParameters>
    {
        public IncomeOverTime(IIncomeOverTimeParameters parameters) : base(parameters) { }

        protected override void Initialise() { }

        public override void Update(TimeSpan elapsedTime)
        {
            if (!Owner.IsCompleted) return;

            Owner.Faction.Resources.ProvideResourcesOverTime(Parameters.IncomePerSecond);
        }

        public override void Draw(GeometryManager geometries) { }

    }
}
