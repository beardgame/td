using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models;

interface ISpawnObjectOnHitParameters : IParametersTemplate<ISpawnObjectOnHitParameters>
{
    IComponentOwnerBlueprint Object { get; }

    bool OnHitLevel { get; }

    bool OnHitEnemy { get; }
}
