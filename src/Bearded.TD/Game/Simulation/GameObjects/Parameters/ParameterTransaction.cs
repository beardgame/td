using System;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameObjects.Parameters;

sealed class ParameterTransaction(IParametersTemplate parameters, AttributeType attribute, Modification modification)
{
    private static readonly IdManager idManager = new();

    public static ParameterTransaction Empty(IParametersTemplate parameters) =>
        new(parameters, AttributeType.None, Modification.Noop);

    private bool isCommitted;
    private Id<Modification> modificationId = Id<Modification>.Invalid;

    public void Commit()
    {
        if (isCommitted)
        {
            throw new InvalidOperationException("Cannot apply transaction more than once.");
        }

        modificationId = idManager.GetNext<Modification>();
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
