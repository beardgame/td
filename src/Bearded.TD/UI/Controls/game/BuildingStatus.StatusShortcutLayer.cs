using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.UI.Shortcuts;
using Bearded.UI.EventArgs;

namespace Bearded.TD.UI.Controls;

sealed partial class BuildingStatus
{
    private sealed class StatusShortcutLayer(
        GameRequestDispatcher requestDispatcher, Player player, BuildingStatus status) : IShortcutLayer
    {
        public bool TryHandleHit(KeyEventArgs eventArgs)
        {
            var matchingStatus = status.statuses.FirstOrDefault(s => s.Spec.Interaction?.ShortcutKey == eventArgs.Key);
            if (matchingStatus is null) return false;
            matchingStatus.Spec.Interaction!.Interact(requestDispatcher, player);
            return true;
        }
    }
}
