using System;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Model;

namespace Bearded.TD.Game.Simulation.Damage;

static class DamageTypeExtensions
{
    public static Element ToElement(this DamageType damageType) => damageType switch
    {
        DamageType.Kinetic => Element.Kinetics,
        DamageType.Fire => Element.Fire,
        DamageType.Lightning => Element.Lightning,
        DamageType.Energy => Element.Energy,
        DamageType.Frost => Element.Water,
        DamageType.Alchemy => Element.Alchemy,
        DamageType.DivineIntervention => throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null),
        _ => throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null)
    };

    public static DamageType ToDamageType(this Element element) => element switch
    {
        Element.Kinetics => DamageType.Kinetic,
        Element.Fire => DamageType.Fire,
        Element.Lightning => DamageType.Lightning,
        Element.Alchemy => DamageType.Alchemy,
        Element.Water => DamageType.Frost,
        Element.Energy => DamageType.Energy,
        _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
    };

    public static Color GetColor(this DamageType damageType) => damageType == DamageType.DivineIntervention
        ? Color.White
        : damageType.ToElement().GetColor();
}
