using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Linq;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class TransactComponents : UpgradeEffectBase
{
    public enum ReplaceMode
    {
        /// <summary>
        /// Remove all components with the given key. Only if there was at least one removal, add the new component.
        /// </summary>
        Replace,

        /// <summary>
        /// Remove all components with the given key. Add the new component regardless of whether any component was
        /// removed.
        /// </summary>
        InsertOrReplace
    }

    private readonly IComponent? componentToAdd;
    private readonly string keyToRemove;
    private readonly ReplaceMode replaceMode;

    public TransactComponents(
        IComponent? componentToAdd,
        string keyToRemove,
        ReplaceMode replaceMode,
        UpgradePrerequisites prerequisites,
        bool isSideEffect)
        : base(prerequisites, isSideEffect)
    {
        this.componentToAdd = componentToAdd;
        this.keyToRemove = keyToRemove;
        this.replaceMode = replaceMode;
    }

    public override bool ModifiesComponentCollection(GameObject subject)
    {
        return (replaceMode == ReplaceMode.InsertOrReplace && componentToAdd is not null) ||
            subject.FindComponents(keyToRemove).Any();
    }

    public override ComponentTransaction CreateComponentChanges(GameObject subject)
    {
        var removals =
            subject.FindComponents(keyToRemove).Select(ComponentCollectionMutation.Removal).ToImmutableArray();
        var canAddComponent = componentToAdd is not null &&
            (!removals.IsDefaultOrEmpty || replaceMode == ReplaceMode.InsertOrReplace);
        var additions = canAddComponent
            ? ComponentCollectionMutation
                .Addition(ComponentFactories.CreateComponentFactory(componentToAdd).Create())
                .Yield()
            : Enumerable.Empty<ComponentCollectionMutation>();

        return new ComponentTransaction(subject, removals.Concat(additions).ToImmutableArray());
    }
}
