using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Elements.Phenomena;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Commands;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[GenerateCommand]
static partial class ObscureVision
{
    private static void execute(GameObject obj, TimeSpan duration, double factor)
    {
        obj.TryApplyEffect(new ObscuredVision.Effect(factor, duration));
    }
}
