using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatus
{
    public IReadonlyBinding<bool> ShowExpanded => showExpanded;

    private readonly Binding<bool> showExpanded = new(false);

    public void PromoteToExpandedView()
    {
        showExpanded.SetFromSource(true);
    }
}
