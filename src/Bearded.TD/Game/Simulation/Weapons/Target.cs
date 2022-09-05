using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Weapons;

readonly record struct Target(GameObject Object);

static class TargetExtensions
{
    public static Target AsTarget(this GameObject obj) => new(obj);
}
