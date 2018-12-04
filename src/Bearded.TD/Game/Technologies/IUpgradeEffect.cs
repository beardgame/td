namespace Bearded.TD.Game.Technologies
{
    interface IUpgradeEffect
    {
        bool CanApplyTo(object subject);
        void ApplyTo(object subject);
    }
}
