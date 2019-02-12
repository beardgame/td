using Bearded.Utilities;

namespace Bearded.TD.Game
{
    interface IMortal
    {
        event GenericEventHandler<int> Damaged;
        event GenericEventHandler<int> Healed;

        void OnDeath();
    }
}
