using Bearded.TD.Game.Elements;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    interface IMortal
    {
        event GenericEventHandler<int, DamageType> Damaged;
        event GenericEventHandler<int> Healed;

        void OnDeath();
    }
}
