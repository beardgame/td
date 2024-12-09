using System;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class ParameterTransaction(
    GameObject gameObject, IParametersTemplate parameters, AttributeType attribute, Modification modification)
{
    public static ParameterTransaction Empty(GameObject gameObject, IParametersTemplate parameters) =>
        new(gameObject, parameters, AttributeType.None, Modification.Noop);

    private bool isCommitted;
    private Id<Modification> modificationId = Id<Modification>.Invalid;

    public void Commit()
    {
        if (isCommitted)
        {
            throw new InvalidOperationException("Cannot apply transaction more than once.");
        }

        modificationId = gameObject.Game.GamePlayIds.GetNext<Modification>();
        var modificationWithId = new ModificationWithId(modificationId, modification);
        parameters.AddModificationWithId(attribute, modificationWithId);

        isCommitted = true;
    }

    public void Rollback()
    {
        if (!isCommitted)
        {
            throw new InvalidOperationException("Cannot roll back component transaction that was not committed.");
        }

        parameters.RemoveModification(attribute, modificationId);

        isCommitted = false;
    }
}
