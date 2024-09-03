using System;
using System.Collections.ObjectModel;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed partial class BuildingStatusControl
{
    private sealed class UpgradeSelectRow : CompositeControl
    {
        private readonly IReadonlyBinding<int?> activeUpgradeSlot;
        private readonly IReadonlyBinding<Resource<Scrap>> currentResources;
        private readonly Action<IPermanentUpgrade> doUpgrade;
        private readonly Binding<bool> upgradeChoicesEnabled = new(true);
        private readonly UIContext uiContext;
        private readonly Control iconRow;

        public UpgradeSelectRow(
            ReadOnlyObservableCollection<IPermanentUpgrade> availableUpgrades,
            IReadonlyBinding<int?> activeUpgradeSlot,
            IReadonlyBinding<Resource<Scrap>> currentResources,
            Action<IPermanentUpgrade> doUpgrade,
            UIContext uiContext)
        {
            this.activeUpgradeSlot = activeUpgradeSlot;
            this.currentResources = currentResources;
            this.doUpgrade = doUpgrade;
            this.uiContext = uiContext;

            iconRow = new IconRow<IPermanentUpgrade>(availableUpgrades, createControl);
            Add(iconRow);
        }

        protected override void OnAddingToParent()
        {
            base.OnAddingToParent();
            onActiveUpgradeSlotChanged(activeUpgradeSlot.Value);
            activeUpgradeSlot.SourceUpdated += onActiveUpgradeSlotChanged;
        }

        protected override void OnRemovingFromParent()
        {
            activeUpgradeSlot.SourceUpdated -= onActiveUpgradeSlotChanged;
            base.OnRemovingFromParent();
        }

        private void onActiveUpgradeSlotChanged(int? index)
        {
            iconRow.Anchor(a => a.Left(buttonLeftMargin(index ?? 0)));
            upgradeChoicesEnabled.SetFromSource(true);
        }

        private Control createControl(IPermanentUpgrade upgrade)
        {
            var resourcesAreSufficient = currentResources.Transform(r => r >= upgrade.Cost);
            return uiContext.Factories.UpgradeChoice(
                upgrade,
                _ => onUpgradeSelected(upgrade),
                resourcesAreSufficient.And(upgradeChoicesEnabled));
        }

        private void onUpgradeSelected(IPermanentUpgrade upgrade)
        {
            // Disable the buttons. We don't want further clicks to be made by clients until we get notified that the
            // upgrade succeeded, and there may be a slight delay in that happening. The "upgrade applied" event chain
            // is responsible for cleaning up this UI (to allow the same actions to happen when other clients choose
            // upgrades).
            upgradeChoicesEnabled.SetFromSource(false);

            doUpgrade(upgrade);
        }
    }
}
