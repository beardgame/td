using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Elements.Phenomena;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Commands;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

[GenerateCommand]
static partial class StunObject
{
    private static void execute(GameObject obj, TimeSpan duration)
    {
        obj.TryApplyEffect(new Stunned.Effect(duration));
    }
}
