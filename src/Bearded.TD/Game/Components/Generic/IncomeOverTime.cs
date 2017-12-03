using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    class IncomeOverTime<T> : Component<T, IncomeOverTimeParameters>
        where T : IFactioned
    {
        public IncomeOverTime(IncomeOverTimeParameters parameters) : base(parameters) { }

        protected override void Initialise() { }

        public override void Update(TimeSpan elapsedTime)
        {
            Owner.Faction.Resources.ProvideResourcesOverTime(Parameters.IncomePerSecond);
        }

        public override void Draw(GeometryManager geometries) { }

    }
}
