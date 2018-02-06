using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Units.StatusEffects
{
    interface IStatusEffectSource
    {
        IUnitStatusEffect Effect { get; }
        bool HasEnded { get; }

        void Update(TimeSpan elapsedTime);
    }
}
