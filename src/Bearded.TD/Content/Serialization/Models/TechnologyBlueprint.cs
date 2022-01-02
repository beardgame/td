using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class TechnologyBlueprint
    : IConvertsTo<Content.Models.TechnologyBlueprint, TechnologyBlueprint.DependencyResolvers>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int Cost { get; set; }
    public List<TechnologyUnlock>? Unlocks { get; set; }
    public List<string>? RequiredTechs { get; set; }

    public Content.Models.TechnologyBlueprint ToGameModel(ModMetadata modMetadata, DependencyResolvers resolvers)
    {
        _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");
        _ = Name ?? throw new InvalidDataException($"{nameof(Name)} must be non-null");

        return new(
            ModAwareId.FromNameInMod(Id, modMetadata),
            Name,
            Cost,
            ImmutableArray.CreateRange(Unlocks
                    ?.Select(u => u.ToGameModel(resolvers.ComponentOwnerResolver, resolvers.UpgradeResolver))
                ?? Enumerable.Empty<ITechnologyUnlock>()),
            RequiredTechs?.Select(resolvers.TechnologyResolver.Resolve)
            ?? Enumerable.Empty<ITechnologyBlueprint>());
    }

    public sealed class DependencyResolvers
    {
        public IDependencyResolver<IComponentOwnerBlueprint> ComponentOwnerResolver { get; }
        public IDependencyResolver<IUpgradeBlueprint> UpgradeResolver { get; }
        public IDependencyResolver<ITechnologyBlueprint> TechnologyResolver { get; }

        public DependencyResolvers(IDependencyResolver<IComponentOwnerBlueprint> componentOwnerResolver,
            IDependencyResolver<IUpgradeBlueprint> upgradeResolver,
            IDependencyResolver<ITechnologyBlueprint> technologyResolver)
        {
            ComponentOwnerResolver = componentOwnerResolver;
            UpgradeResolver = upgradeResolver;
            TechnologyResolver = technologyResolver;
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class TechnologyUnlock
    {
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public enum UnlockType
        {
            Unknown = 0,
            Building = 1,
            Upgrade = 2,
        }

        public UnlockType Type { get; set; }
        public string? Blueprint { get; set; }

        public ITechnologyUnlock ToGameModel(
            IDependencyResolver<IComponentOwnerBlueprint> buildingResolver,
            IDependencyResolver<IUpgradeBlueprint> upgradeResolver)
        {
            _ = Blueprint ?? throw new InvalidDataException($"{nameof(Blueprint)} must be non-null");

            return Type switch
            {
                UnlockType.Building => new BuildingUnlock(buildingResolver.Resolve(Blueprint)),
                UnlockType.Upgrade => new UpgradeUnlock(upgradeResolver.Resolve(Blueprint)),
                _ => throw new InvalidDataException($"Invalid unlock type: {Type}")
            };
        }
    }
}