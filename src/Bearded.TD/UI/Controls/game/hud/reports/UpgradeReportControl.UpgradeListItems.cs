using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls;

sealed partial class UpgradeReportControl
{
    private sealed class UpgradeListItems : IListItemSource
    {
        private readonly Dictionary<int, Binding<double>> progressBindings = new();
        public ImmutableArray<IUpgradeReportInstance.IUpgradeModel> AppliedUpgrades { get; }
        private readonly Binding<bool> canUpgrade;
        private readonly Binding<string> slots;

        public int ItemCount => AppliedUpgrades.Length + 2;

        public event VoidEventHandler? ChooseUpgradeButtonClicked;

        public UpgradeListItems(
            ImmutableArray<IUpgradeReportInstance.IUpgradeModel> upgrades,
            Binding<bool> canUpgrade,
            Binding<string> slots)
        {
            AppliedUpgrades = upgrades;
            this.canUpgrade = canUpgrade;
            this.slots = slots;
        }

        public double HeightOfItemAt(int index) => index == 0 ? Constants.UI.Text.LineHeight : Constants.UI.Button.Height;

        public Control CreateItemControlFor(int index)
        {
            if (index == 0)
            {
                return TextFactories.ValueLabel("Slots used", slots);
            }
            if (index == AppliedUpgrades.Length + 1)
            {
                return ButtonFactories.Button(b =>
                {
                    return b
                        .WithLabel("Choose upgrade")
                        .WithEnabled(canUpgrade)
                        .WithOnClick(() => ChooseUpgradeButtonClicked?.Invoke());
                });
            }

            var model = AppliedUpgrades[index - 1];
            var progressBinding = model.IsFinished ? null : Binding.Create(model.Progress);
            if (progressBinding != null)
            {
                progressBindings[index - 1] = progressBinding;
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
