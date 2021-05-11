using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    enum SpringBehavior
    {
        Push,
        Pull,
    }

    record Spring(Circle Circle1, Circle Circle2, SpringBehavior Behavior,
        float ForceMultiplier = 1, Unit Overlap = default)
    {
        public Unit TargetDistance => Circle1.Radius + Circle2.Radius - Overlap;
    }

}
