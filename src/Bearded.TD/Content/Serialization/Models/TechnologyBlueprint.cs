using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class TechnologyBlueprint
        : IConvertsTo<Content.Models.TechnologyBlueprint, TechnologyBlueprint.DependencyResolvers>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public List<TechnologyUnlock> Unlocks { get; set; }
        public List<string> RequiredTechs { get; set; } = new();

        public Content.Models.TechnologyBlueprint ToGameModel(ModMetadata modMetadata, DependencyResolvers resolvers)
        {
            return new(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Name,
                Cost,
                ImmutableArray.CreateRange(Unlocks.Select(u => u.ToGameModel(resolvers.BuildingResolver, resolvers.UpgradeResolver))),
                RequiredTechs.Select(resolvers.TechnologyResolver.Resolve));
        }

        public sealed class DependencyResolvers
        {
            public IDependencyResolver<IBuildingBlueprint> BuildingResolver { get; }
            public IDependencyResolver<IUpgradeBlueprint> UpgradeResolver { get; }
            public IDependencyResolver<ITechnologyBlueprint> TechnologyResolver { get; }

            public DependencyResolvers(IDependencyResolver<IBuildingBlueprint> buildingResolver,
                IDependencyResolver<IUpgradeBlueprint> upgradeResolver,
                IDependencyResolver<ITechnologyBlueprint> technologyResolver)
            {
                BuildingResolver = buildingResolver;
                UpgradeResolver = upgradeResolver;
                TechnologyResolver = technologyResolver;
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        public sealed class TechnologyUnlock
        {
            public enum UnlockType
            {
                Unknown = 0,
                Building = 1,
                Upgrade = 2,
            }

            public UnlockType Type { get; set; }
            public string Blueprint { get; set; }

            public ITechnologyUnlock ToGameModel(
                IDependencyResolver<IBuildingBlueprint> buildingResolver,
                IDependencyResolver<IUpgradeBlueprint> upgradeResolver)
            {
                switch (Type)
                {
                    case UnlockType.Building:
                        return new BuildingUnlock(buildingResolver.Resolve(Blueprint));
                    case UnlockType.Upgrade:
                        return new UpgradeUnlock(upgradeResolver.Resolve(Blueprint));
                    default:
                        throw new InvalidDataException($"Invalid unlock type: {Type}");
                }
            }
        }
    }
}
