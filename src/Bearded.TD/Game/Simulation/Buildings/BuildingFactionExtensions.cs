using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Buildings;

static class BuildingFactionExtensions
{
    public static bool CanBeManuallyControlledBy(this GameObject gameObj, Faction faction) =>
        gameObj.FindFaction().OwnedBuildingsCanBeManuallyControlledBy(faction);

    public static bool OwnedBuildingsCanBeManuallyControlledBy(this Faction ownerFaction, Faction faction) =>
        ownerFaction.SharesBehaviorWith<FactionResources>(faction);

    public static bool CanBeDeletedBy(this GameObject gameObj, Faction faction) =>
        gameObj.FindFaction().OwnedBuildingsCanBeDeletedBy(faction);

    public static bool OwnedBuildingsCanBeDeletedBy(this Faction ownerFaction, Faction faction) =>
        ownerFaction.SharesBehaviorWith<FactionResources>(faction);

    public static bool CanBeUpgradedBy(this GameObject gameObj, Faction faction) =>
        gameObj.FindFaction().OwnedBuildingsCanBeUpgradedBy(faction);

    public static bool OwnedBuildingsCanBeUpgradedBy(this Faction ownerFaction, Faction faction) =>
        ownerFaction.SharesBehaviorWith<FactionResources>(faction);

    public static void TryRefund(this GameObject gameObj)
    {
        if (!gameObj.TryFindFaction(out var faction))
            return;

        var state = gameObj.GetComponents<IBuildingStateProvider>().Single().State;
        if (!state.IsMaterialized)
            return;

        if (gameObj.GetComponents<BuildingConstructionWork>().SingleOrDefault() is { } constructionWork)
        {
            refund(faction, constructionWork.ResourcesInvestedSoFar ?? ResourceAmount.Zero);
        }
        else if(gameObj.GetComponents<ICost>().SingleOrDefault() is { } cost)
        {
            refund(faction, cost.Resources);
        }

        static void refund(Faction faction, ResourceAmount value)
        {
            if (!faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
                throw new InvalidOperationException();
            resources.ProvideResources(value);
        }
    }

    public static Faction FindFaction(this GameObject gameObj)
    {
        if (!gameObj.TryFindFaction(out var faction))
        {
            throw new InvalidOperationException($"Could not find faction on game object {gameObj}");
        }

        return faction;
    }

    public static bool TryFindFaction(this GameObject gameObj, [NotNullWhen(true)] out Faction? faction)
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
