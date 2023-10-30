using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

abstract class AddComponent : UpgradeEffectBase
{
    public override bool ModifiesComponentCollection => true;

    protected AddComponent(UpgradePrerequisites prerequisites, bool isSideEffect)
        : base(prerequisites, isSideEffect) { }

    public override ComponentTransaction CreateComponentChanges(GameObject gameObject)
    {
        return new ComponentTransaction(gameObject,
            ImmutableArray.Create(
                ComponentCollectionMutation.Addition(CreateComponent())));
    }

    protected abstract IComponent CreateComponent();
}
