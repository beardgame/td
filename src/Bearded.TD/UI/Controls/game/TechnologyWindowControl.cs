using System;
using System.Linq;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Controls;

sealed class TechnologyWindowControl : CompositeControl
{
    private const double contentWidth = 960;
    private const double contentHeight = 720;
    private const double minRowHeight = 48;

    private readonly TechnologyWindow model;

    public TechnologyWindowControl(TechnologyWindow model)
    {
        this.model = model;
        IsClickThrough = true;
        Add(WindowFactories
            .Window(b => b.WithOnClose(model.CloseWindow).WithContent(buildContent()))
            .AnchorAsWindow(contentWidth, contentHeight));
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

    private Control buildContent()
    {
        var control = new CompositeControl();
        control.BuildFixedColumn()
            .Add(buildHeaderRow(), minRowHeight)
            .Add(buildBranchRows(), contentHeight - minRowHeight);
        return control;
    }

    private Control buildHeaderRow()
    {
        var control = new CompositeControl();
        control.BuildFixedRow()
            .AddLeft(new EmptyControl(), 0.25 * contentWidth)
            .AddColumnHeader("I", 0.25 * contentWidth)
            .AddColumnHeader("II", 0.25 * contentWidth)
            .AddColumnHeader("III", 0.25 * contentWidth);
        return control;
    }

    private Control buildBranchRows()
    {
        var control = new CompositeControl();
        var column = control.BuildScrollableColumn();
        foreach (var branch in Enum.GetValues<TechnologyBranch>())
        {
            var row = buildBranchRow(branch, out var height);
            column.Add(row, height);
        }
        return control;
    }

    private Control buildBranchRow(TechnologyBranch branch, out double height)
    {
        var shownTiers = new[] { TechnologyTier.Low, TechnologyTier.Medium, TechnologyTier.High };

        var branchModel = model.TechTree.Branches[branch];

        var control = new CompositeControl();
        var row = control.BuildFixedRow()
            .AddHeaderLeft($"{branch}", 0.25 * contentWidth, color: branch.GetColor());
        height = minRowHeight;

        if (shownTiers.All(t => branchModel.Tiers[t].Technologies.IsEmpty))
        {
            row.AddLabelLeft("Coming soon!", 0.25 * contentWidth);
            return control;
        }

        foreach (var tier in shownTiers)
        {
            var tierModel = branchModel.Tiers[tier];
            var techList = tierModel.Technologies;

            var buttons = new CompositeControl();
            var buttonColumn = buttons.BuildFixedColumn();
            foreach (var tech in techList)
            {
                buttonColumn.AddCenteredButton(b => b
                    .WithLabel(tech.Blueprint.Name)
                    .WithEnabled(tech.IsUnlockedBinding.Negate().And(model.CanUnlockTechnologyNowBinding))
                    .WithOnClick(args =>
                    {
                        if (Constants.Input.DebugForceModifier(args.ModifierKeys))
                        {
                            model.ForceTechnologyUnlock(tech.Blueprint);
                        }
                        else
                        {
                            model.RequestTechnologyUnlock(tech.Blueprint);
                        }
                    }));
            }

            var tierControl = new CompositeControl
            {
                new BackgroundBox(branch.GetColor() * 0.1f),
                ProgressBarFactories.BareProgressBar(tierModel.CompletionPercentageBinding, branch.GetColor() * 0.15f),
                buttons.SurroundWithMargins(LayoutMargin)
            };

            row.AddLeft(
                tierControl.SurroundWithMargins(0.5 * LayoutMarginSmall),
                0.25 * contentWidth);
            height = Math.Max(height, buttonColumn.Height + 2 * LayoutMargin + LayoutMarginSmall);
        }

        return control;
    }
}
