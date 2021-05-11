using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    enum SpringBehavior
    {
        Push,
        Pull,
    }

    record Spring(FeatureCircle Circle1, FeatureCircle Circle2, SpringBehavior Behavior,
        float ForceMultiplier = 1, Unit Overlap = default)
    {
        public Unit TargetDistance => Circle1.Circle.Radius + Circle2.Circle.Radius - Overlap;
    }
}
