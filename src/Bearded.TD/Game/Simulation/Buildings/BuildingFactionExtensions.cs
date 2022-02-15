using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Buildings;

static class BuildingFactionExtensions
{
    public static bool CanBeManuallyControlledBy(this ComponentGameObject gameObj, Faction faction) =>
        gameObj.FindFaction().OwnedBuildingsCanBeManuallyControlledBy(faction);

    public static bool OwnedBuildingsCanBeManuallyControlledBy(this Faction ownerFaction, Faction faction) =>
        ownerFaction.SharesBehaviorWith<FactionResources>(faction);

    public static bool CanBeDeletedBy(this ComponentGameObject gameObj, Faction faction) =>
        gameObj.FindFaction().OwnedBuildingsCanBeDeletedBy(faction);

    public static bool OwnedBuildingsCanBeDeletedBy(this Faction ownerFaction, Faction faction) =>
        ownerFaction.SharesBehaviorWith<FactionResources>(faction);

    public static bool CanBeUpgradedBy(this ComponentGameObject gameObj, Faction faction) =>
        gameObj.FindFaction().OwnedBuildingsCanBeUpgradedBy(faction);

    public static bool OwnedBuildingsCanBeUpgradedBy(this Faction ownerFaction, Faction faction) =>
        ownerFaction.SharesBehaviorWith<FactionResources>(faction);

    public static Faction FindFaction(this ComponentGameObject gameObj)
    {
        if (!gameObj.TryFindFaction(out var faction))
        {
            throw new InvalidOperationException($"Could not find faction on game object {gameObj}");
        }

        return faction;
    }

    public static bool TryFindFaction(this ComponentGameObject gameObj, [NotNullWhen(true)] out Faction? faction)
    {
        var factionProvider = gameObj.GetComponents<IFactionProvider>().FirstOrDefault();
        if (factionProvider == default)
        {
            faction = null;
            return false;
        }

        faction = factionProvider.Faction;
        return true;
    }
}