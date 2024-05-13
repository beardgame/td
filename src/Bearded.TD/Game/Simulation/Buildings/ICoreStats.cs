using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Buildings;

interface ICoreStats : IDeletable
{
    GameObject Object { get; }
    HitPoints CurrentHealth { get; }
    HitPoints MaxHealth { get; }
    EMPStatus EMPStatus { get; }
}

enum EMPStatus
{
    Absent,
    Recharging,
    Ready
}
