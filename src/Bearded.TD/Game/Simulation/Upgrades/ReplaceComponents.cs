using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class ReplaceComponents : UpgradeEffectBase
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

    private readonly ImmutableArray<IComponent> componentsToAdd;
    private readonly string keyToRemove;
    private readonly ReplaceMode replaceMode;

    public ReplaceComponents(
        ImmutableArray<IComponent> componentsToAdd,
        string keyToRemove,
        ReplaceMode replaceMode,
        UpgradePrerequisites prerequisites,
        bool isSideEffect)
        : base(prerequisites, isSideEffect)
    {
        this.componentsToAdd = componentsToAdd;
        this.keyToRemove = keyToRemove;
        this.replaceMode = replaceMode;
    }

    public override bool ModifiesComponentCollection(GameObject subject)
    {
        return (replaceMode == ReplaceMode.InsertOrReplace && !componentsToAdd.IsEmpty) ||
            subject.FindComponents(keyToRemove).Any();
    }

    public override ComponentTransaction CreateComponentChanges(GameObject subject)
    {
        var removals =
            subject.FindComponents(keyToRemove).Select(ComponentCollectionMutation.Removal).ToImmutableArray();
        var canAddComponent = !removals.IsDefaultOrEmpty || replaceMode == ReplaceMode.InsertOrReplace;
        var additions = canAddComponent
            ? componentsToAdd
                .Select(ComponentFactories.CreateComponentFactory)
                .Select(c => c.Create())
                .Select(ComponentCollectionMutation.Addition)
            : Enumerable.Empty<ComponentCollectionMutation>();

        return new ComponentTransaction(subject, removals.Concat(additions).ToImmutableArray());
    }
}
