using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Enemies;

sealed record EnemyFormRecipe(IGameObjectBlueprint Blueprint, ImmutableDictionary<SocketShape, ModAwareId> Modules)
{
    public EnemyForm ToForm(Blueprints blueprints)
    {
        var resolvedModules = Modules.ToImmutableDictionary(kvp => kvp.Key, kvp => blueprints.Modules[kvp.Value]);

        // NOTE: not ideal that we are instantiating an enemy here, but there is no easy way to get to the summary that
        // we share in the enemy form generator. Perhaps the solution here is to have a central cache of instantiated
        // blueprints. For now... meh.
        var template = EnemyFactory.CreateTemplate(Blueprint);
        var resistanceContributions = template.GetComponents<IResistanceContributions>().SingleOrDefault();
        var resistances =
            DamageResistancesCalculator.DeriveResistancesFromModules(resolvedModules, resistanceContributions);

        return new EnemyForm(Blueprint, resolvedModules, resistances);
    }
}
