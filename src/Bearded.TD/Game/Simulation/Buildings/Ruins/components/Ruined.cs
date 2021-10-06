using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins
{
    [Component("ruined")]
    sealed class Ruined<T> : Component<T, IRuinedParameters>
    {
        public Ruined(IRuinedParameters parameters) : base(parameters) { }

        protected override void OnAdded()
        {
            Events.Send(new ObjectRuined());
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }
}
