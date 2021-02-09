using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class UpgradeReportControl : ReportControl
    {
        public override double Height { get; } = 200;

        private readonly IUpgradeReportInstance reportInstance;

        private readonly ListControl list;
        private UpgradeListItems listItems;

        public UpgradeReportControl(IUpgradeReportInstance reportInstance)
        {
            this.reportInstance = reportInstance;
            reportInstance.UpgradesUpdated += updateUpgradesFromReport;

            list = new ListControl(new ViewportClippingLayerControl());
            listItems = new UpgradeListItems(
                reportInstance.Upgrades.ToImmutableArray(), reportInstance.CanPlayerUpgradeBuilding);
            listItems.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
            list.ItemSource = listItems;
            Add(list);
        }

        private void updateUpgradesFromReport()
        {
            var newUpgrades = reportInstance.Upgrades.ToImmutableArray();

            if (listItems.ItemCount - 1 == newUpgrades.Length)
            {
                return;
            }

            listItems.ChooseUpgradeButtonClicked -= onChooseUpgradeButtonClicked;
            listItems.DestroyAll();
            listItems = new UpgradeListItems(newUpgrades, reportInstance.CanPlayerUpgradeBuilding);
            listItems.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
            list.ItemSource = listItems;
        }

        public override void Update()
        {
            listItems.UpdateProgress();
        }

        public override void Dispose()
        {
            reportInstance.Dispose();
        }

        private void onChooseUpgradeButtonClicked()
        {
            throw new NotImplementedException();
        }

        private sealed class UpgradeListItems : IListItemSource
        {
            private readonly Dictionary<int, Binding<double>> progressBindings = new();
            private readonly ImmutableArray<IUpgradeReportInstance.IUpgradeModel> appliedUpgrades;
            private readonly bool canUpgrade;

            public int ItemCount => appliedUpgrades.Length + 1;

            public event VoidEventHandler? ChooseUpgradeButtonClicked;

            public UpgradeListItems(
                ImmutableArray<IUpgradeReportInstance.IUpgradeModel> upgrades, bool canUpgrade)
            {
                appliedUpgrades = upgrades;
                this.canUpgrade = canUpgrade;
            }

            public double HeightOfItemAt(int index) => Constants.UI.Button.Height;

            public Control CreateItemControlFor(int index)
            {
                if (index == appliedUpgrades.Length)
                {
                    return ButtonFactories.Button(b =>
                    {
                        b.WithLabel("Choose upgrade");
                        if (canUpgrade)
                        {
                            b.WithOnClick(() => ChooseUpgradeButtonClicked?.Invoke());
                        }
                        else
                        {
                            b.MakeDisabled();
                        }
                        return b;
                    });
                }

                var model = appliedUpgrades[index];
                var progressBinding = model.IsFinished ? null : Binding.Create(model.Progress);
                if (progressBinding != null)
                {
                    progressBindings[index] = progressBinding;
                }
                return ButtonFactories.Button(b =>
                {
                    b.WithLabel(model.Blueprint.Name);
                    if (progressBinding == null)
                    {
                        b.MakeDisabled();
                    }
                    else
                    {
                        b.WithProgressBar(progressBinding);
                    }
                    return b;
                });
            }

            public void UpdateProgress()
            {
                foreach (var (i, binding) in progressBindings)
                {
                    binding.SetFromSource(appliedUpgrades[i].Progress);
                }
            }

            public void DestroyAll()
            {
                progressBindings.Clear();
            }

            public void DestroyItemControlAt(int index, Control control)
            {
                progressBindings.Remove(index);
            }
        }
    }
}
