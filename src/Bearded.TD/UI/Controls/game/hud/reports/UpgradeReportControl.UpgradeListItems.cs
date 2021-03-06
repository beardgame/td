using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed partial class UpgradeReportControl
    {
        private sealed class UpgradeListItems : IListItemSource
        {
            private readonly Dictionary<int, Binding<double>> progressBindings = new();
            public ImmutableArray<IUpgradeReportInstance.IUpgradeModel> AppliedUpgrades { get; }
            private readonly bool canUpgrade;

            public int ItemCount => AppliedUpgrades.Length + 1;

            public event VoidEventHandler? ChooseUpgradeButtonClicked;

            public UpgradeListItems(
                ImmutableArray<IUpgradeReportInstance.IUpgradeModel> upgrades, bool canUpgrade)
            {
                AppliedUpgrades = upgrades;
                this.canUpgrade = canUpgrade;
            }

            public double HeightOfItemAt(int index) => Constants.UI.Button.Height;

            public Control CreateItemControlFor(int index)
            {
                if (index == AppliedUpgrades.Length)
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

                var model = AppliedUpgrades[index];
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
                    binding.SetFromSource(AppliedUpgrades[i].Progress);
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
