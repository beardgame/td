using System;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Model;

namespace Bearded.TD.Game.Simulation.Damage;

static class DamageTypeExtensions
{
    public static Element ToElement(this DamageType damageType) => damageType switch
    {
        DamageType.Kinetic => Element.Dynamics,
        DamageType.Fire => Element.Combustion,
        DamageType.Electric => Element.Conductivity,
        DamageType.Energy => Element.Photonics,
        DamageType.Frost => Element.Hydrology,
        DamageType.Alchemy => Element.Alchemy,
        DamageType.DivineIntervention => throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null),
        _ => throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null)
    };

    public static DamageType ToDamageType(this Element element) => element switch
    {
        Element.Dynamics => DamageType.Kinetic,
        Element.Combustion => DamageType.Fire,
        Element.Conductivity => DamageType.Electric,
        Element.Alchemy => DamageType.Alchemy,
        Element.Hydrology => DamageType.Frost,
        Element.Photonics => DamageType.Energy,
        _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
    };

    public static Color GetColor(this DamageType damageType) => damageType == DamageType.DivineIntervention
        ? Color.White
        : damageType.ToElement().GetColor();
}
