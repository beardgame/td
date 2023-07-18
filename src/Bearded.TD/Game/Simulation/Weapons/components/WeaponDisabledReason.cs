namespace Bearded.TD.Game.Simulation.Weapons;

interface IWeaponDisabledReason
{
    bool IsResolved { get; }
}

public sealed class WeaponDisabledReason : IWeaponDisabledReason
{
    public bool IsResolved { get; private set; }

    public void Resolve()
    {
        IsResolved = true;
    }
}
