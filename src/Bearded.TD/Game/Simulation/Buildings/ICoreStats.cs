using Bearded.TD.Game.Simulation.Damage;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Buildings;

interface ICoreStats : IDeletable
{
    HitPoints CurrentHealth { get; }
    HitPoints MaxHealth { get; }
    EMPStatus EMPStatus { get; }
    void FireEMP();
}

enum EMPStatus
{
    Absent,
    Recharging,
    Ready
}
