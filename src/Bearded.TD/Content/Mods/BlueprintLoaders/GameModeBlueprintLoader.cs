using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities;
using GameModeBlueprintJson = Bearded.TD.Content.Serialization.Models.GameModeBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    sealed class GameModeBlueprintLoader
        : BaseBlueprintLoader<IGameModeBlueprint, GameModeBlueprintJson, Void>
    {
        protected override string RelativePath => "defs/gamemodes";

        public GameModeBlueprintLoader(BlueprintLoadingContext context) : base(context)
        {
        }

        public override ReadonlyBlueprintCollection<IGameModeBlueprint> LoadBlueprints()
        {
            var nodeGroupConverter = new NodeGroupConverter(Context.FindDependencyResolver<INodeBlueprint>());
            var technologyUnlockConverter = new TechnologyUnlockConverter(
                Context.FindDependencyResolver<IBuildingBlueprint>(),
                Context.FindDependencyResolver<IUpgradeBlueprint>());

            Context.Serializer.Converters.Add(nodeGroupConverter);
            Context.Serializer.Converters.Add(technologyUnlockConverter);

            var blueprints = base.LoadBlueprints();

            Context.Serializer.Converters.Remove(nodeGroupConverter);
            Context.Serializer.Converters.Remove(technologyUnlockConverter);

            return blueprints;
        }
    }
}
