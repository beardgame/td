using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Audio;
using Bearded.TD.Content;
using Bearded.TD.Content.Mods;
using static Bearded.TD.Constants.Content.CoreUI.Sounds;
using static Bearded.TD.Constants.Game.GameUI;

namespace Bearded.TD.Game.Simulation.Model;

static class ElementExtensions
{
    public static IEnumerable<Element> Enumerate() => Enum.GetValues<Element>();

    public static Color GetColor(this Element element) => element switch
    {
        Element.Kinetics => KineticsColor,
        Element.Fire => FireColor,
        Element.Lightning => LightningColor,
        Element.Alchemy => AlchemyColor,
        Element.Water => WaterColor,
        Element.Energy => EnergyColor,
        _ => Color.Pink
    };

    public static ModAwareId GetUpgradeSoundId(this Element element) => element switch
    {
        Element.Kinetics => UpgradeKinetics,
        Element.Fire => UpgradeFire,
        Element.Lightning => UpgradeLightning,
        _ => UpgradeGeneric
    };

    public static ISoundEffect GetUpgradeSound(this Element element, ContentManager contentManager) =>
        contentManager.ResolveSoundEffect(GetUpgradeSoundId(element));
}
