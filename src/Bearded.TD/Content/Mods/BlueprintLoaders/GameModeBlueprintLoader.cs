using System.IO;
using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Upgrades;
using GameModeBlueprintJson = Bearded.TD.Content.Serialization.Models.GameModeBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    sealed class GameModeBlueprintLoader
        : BaseBlueprintLoader<IGameModeBlueprint, GameModeBlueprintJson, GameModeBlueprintJson.DependencyResolvers>
    {
        protected override string RelativePath => "defs/gamemodes";

        public GameModeBlueprintLoader(BlueprintLoadingContext context) : base(context)
        {
        }

        public override ReadonlyBlueprintCollection<IGameModeBlueprint> LoadBlueprints()
        {
            var converter = new TechnologyUnlockConverter(
                Context.FindDependencyResolver<IBuildingBlueprint>(),
                Context.FindDependencyResolver<IUpgradeBlueprint>());
            Context.Serializer.Converters.Add(converter);

            var blueprints = base.LoadBlueprints();

            Context.Serializer.Converters.Remove(converter);

            return blueprints;
        }

        protected override GameModeBlueprintJson.DependencyResolvers GetDependencyResolvers(FileInfo file)
        {
            return new(Context.FindDependencyResolver<INodeBlueprint>());
        }
    }
}
