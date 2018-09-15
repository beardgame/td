using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("incomeOverTime")]
    class IncomeOverTime<T> : Component<T, IIncomeOverTimeParameters>
        where T : IFactioned
    {
        public IncomeOverTime(IIncomeOverTimeParameters parameters) : base(parameters) { }

        protected override void Initialise() { }

        public override void Update(TimeSpan elapsedTime)
        {
            Owner.Faction.Resources.ProvideResourcesOverTime(Parameters.IncomePerSecond);
        }

        public override void Draw(GeometryManager geometries) { }

    }
}
