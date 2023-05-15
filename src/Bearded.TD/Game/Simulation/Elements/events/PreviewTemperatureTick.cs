using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.events;

readonly record struct PreviewTemperatureTick(Instant Now, TemperatureRate Rate) : IComponentPreviewEvent
{
    public PreviewTemperatureTick WithAddedRate(TemperatureRate rate) => this with { Rate = Rate + rate };
}
