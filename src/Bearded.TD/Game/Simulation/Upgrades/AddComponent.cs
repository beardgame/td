using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

abstract class AddComponent : UpgradeEffectBase
{
    protected AddComponent(UpgradePrerequisites prerequisites, bool isSideEffect)
        : base(prerequisites, isSideEffect) { }

    public override bool ModifiesComponentCollection(GameObject subject) => true;

    public override ComponentTransaction CreateComponentChanges(GameObject subject)
    {
        return new ComponentTransaction(subject,
            ImmutableArray.Create(
                ComponentCollectionMutation.Addition(CreateComponent())));
    }

    protected abstract IComponent CreateComponent();
}
