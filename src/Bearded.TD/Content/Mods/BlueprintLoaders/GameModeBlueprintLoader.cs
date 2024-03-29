using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities;
using GameModeBlueprintJson = Bearded.TD.Content.Serialization.Models.GameModeBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class GameModeBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<IGameModeBlueprint, GameModeBlueprintJson, Void>(context)
{
    protected override string RelativePath => "defs/gamemodes";
    protected override DependencySelector? SelectDependency => null;

    public override ReadonlyBlueprintCollection<IGameModeBlueprint> LoadBlueprints()
    {
        var nodeGroupConverter = new NodeGroupConverter(Context.FindDependencyResolver<INodeBlueprint>());
        var technologyUnlockConverter = new TechnologyUnlockConverter(
            Context.FindDependencyResolver<IGameObjectBlueprint>(),
            Context.FindDependencyResolver<IPermanentUpgrade>());
        var gameObjectConverter =
            new DependencyConverter<IGameObjectBlueprint>(Context.FindDependencyResolver<IGameObjectBlueprint>());

        Context.Serializer.Converters.Add(gameObjectConverter);
        Context.Serializer.Converters.Add(nodeGroupConverter);
        Context.Serializer.Converters.Add(technologyUnlockConverter);

        var blueprints = base.LoadBlueprints();

        Context.Serializer.Converters.Remove(gameObjectConverter);
        Context.Serializer.Converters.Remove(nodeGroupConverter);
        Context.Serializer.Converters.Remove(technologyUnlockConverter);

        return blueprints;
    }
}
