using System;
using System.Collections.Immutable;
using System.Linq;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class ComponentTransaction
{
    public static ComponentTransaction Empty(GameObject gameObject) =>
        new(gameObject, ImmutableArray<ComponentCollectionMutation>.Empty);

    private readonly GameObject gameObject;
    private readonly ImmutableArray<ComponentCollectionMutation> mutations;
    private bool isCommitted;

    public ComponentTransaction(GameObject gameObject, ImmutableArray<ComponentCollectionMutation> mutations)
    {
        this.gameObject = gameObject;
        this.mutations = mutations;
    }

    public void Commit()
    {
        if (isCommitted)
        {
            throw new InvalidOperationException("Cannot apply transaction more than once.");
        }

        gameObject.ModifyComponentCollection(mutations);

        isCommitted = true;
    }

    public void Rollback()
    {
        if (!isCommitted)
        {
            throw new InvalidOperationException("Cannot roll back component transaction that was not committed.");
        }

        gameObject.ModifyComponentCollection(mutations.Select(m => m.Reversed()));

        isCommitted = false;
    }
}
