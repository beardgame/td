using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.Damage;

interface IHitPointsPoolDisabledReceipt
{
    void Undo();
}

sealed partial class HitPointsPool
{
    private readonly List<DisabledReceipt> disabledReceipts = [];

    public bool IsDisabled => disabledReceipts.Count > 0;

    public IHitPointsPoolDisabledReceipt Disable()
    {
        var receipt = new DisabledReceipt(this);
        disabledReceipts.Add(receipt);
        return receipt;
    }

    private sealed class DisabledReceipt(HitPointsPool pool) : IHitPointsPoolDisabledReceipt
    {
        public void Undo()
        {
            pool.disabledReceipts.Remove(this);
        }
    }
}
