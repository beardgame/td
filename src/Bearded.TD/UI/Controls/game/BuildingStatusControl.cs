using Bearded.TD.Game.Commands;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.UI.BuildingStatus;

namespace Bearded.TD.UI.Controls;

sealed partial class BuildingStatusControl : CompositeControl
{
    private static readonly Vector2d buttonBetweenMargin = (Constants.UI.Button.Margin, Constants.UI.Button.Margin * 3);
    private static readonly double rowHeight = ButtonSize + buttonBetweenMargin.Y;
    private static double buttonLeftMargin(int i) => i * (ButtonSize + buttonBetweenMargin.X);

    public Vector2d Size { get; }

    public BuildingStatusControl(
        BuildingStatus model,
        Animations animations,
        TooltipFactory tooltipFactory,
        GameRequestDispatcher requestDispatcher)
    {
        // TODO: UI library doesn't allow for this to apply to all nested elements, which is really what we need...
        this.BindIsClickThrough(model.ShowExpanded.Negate());
        Add(new BlurBackground());
        this.Add(new ComplexBox
        {
            Components = Background,
            CornerRadius = 5,
        }.WithDropShadow(Shadow, ShadowFade));

        var innerContainer = new CompositeControl();
        Add(innerContainer.Anchor(a => a.MarginAllSides(Padding).Top()));

        var column = innerContainer.BuildFixedColumn();
        column.AddHeader(Binding.Constant(model.BuildingName));

        if (model.ShowVeterancy)
        {
            column.Add(new VeterancyRow(model.Veterancy, animations), Veterancy.RowHeight);
        }

        column
            .Add(new IconRow<ObservableStatus>(
                    model.Statuses,
                    status => StatusIconFactories.StatusIcon(status, animations, requestDispatcher),
                    StatusRowBackground),
                rowHeight);

        if (model.ShowUpgrades)
        {
            column
                .Add(new IconRow<IReadonlyBinding<UpgradeSlot>>(
                        model.Upgrades,
                        slot => StatusIconFactories.UpgradeSlot(
                            slot,
                            model.AvailableUpgrades.IsCountPositive()
                                .And(Binding.Combine(slot, model.ActiveUpgradeSlot, (s, i) => s.Index == i)),
                            model.ToggleUpgradeSelect,
                            animations,
                            tooltipFactory)),
                    rowHeight)
                .Add(new UpgradeSelectRow(
                        model.AvailableUpgrades,
                        model.ActiveUpgradeSlot,
                        model.CurrentResources,
                        model.ApplyUpgrade,
                        animations,
                        tooltipFactory).BindIsVisible(model.ShowUpgradeSelect),
                    rowHeight);
        }

        if (model.ShowDeletion)
        {
            column
                .AddLeftButton(b => b
                    .WithLabel("Delete")
                    .WithOnClick(model.DeleteBuilding));
        }

        Size = (300, column.Height + Padding);
    }
}
