using System.Text.Json;
using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities;
using GameModeBlueprintJson = Bearded.TD.Content.Serialization.Models.GameModeBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    sealed class GameModeBlueprintLoader : BaseBlueprintLoader<IGameModeBlueprint, GameModeBlueprintJson, Void>
    {
        protected override string RelativePath => "defs/gamemodes";

        protected override JsonSerializerOptions SerializerOptions
        {
            get
            {
                var options = new JsonSerializerOptions(base.SerializerOptions);
                options.Converters.Add(new TechnologyUnlockConverter(
                    Context.FindDependencyResolver<IBuildingBlueprint>(),
                    Context.FindDependencyResolver<IUpgradeBlueprint>()));
                return options;
            }
        }

        public GameModeBlueprintLoader(BlueprintLoadingContext context) : base(context) { }
    }
}
