using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatusControl : CompositeControl
{
    private const double headerSize = Constants.UI.Text.HeaderLineHeight;
    private const double buttonSize = Constants.UI.BuildingStatus.ButtonSize;
    private static readonly Vector2d buttonBetweenMargin = (Constants.UI.Button.Margin, Constants.UI.Button.Margin * 3);
    private static double buttonLeftMargin(int i) => i * (buttonSize + buttonBetweenMargin.X);

    public static readonly Vector2d Size =
    (
        300,
        headerSize + 2 * (buttonSize + buttonBetweenMargin.Y)
    );

    public BuildingStatusControl(BuildingStatus model)
    {
        // TODO: UI library doesn't allow for this to apply to all nested elements, which is really what we need...
        this.BindIsClickThrough(model.ShowExpanded.Negate());
        Add(new ComplexBox
        {
            Components = Constants.UI.BuildingStatus.Background,
            CornerRadius = 5,
        });

        var innerContainer = new CompositeControl();
        Add(innerContainer.Anchor(a => a.MarginAllSides(Constants.UI.BuildingStatus.Padding).Top()));

        var column = innerContainer.BuildFixedColumn();
        column
            .AddHeader(model.ShowExpanded.Transform(b => b ? "Expanded" : "Preview"))
            .Add(new IconRow<Status>(
                    model.Statuses, StatusIconFactories.StatusIcon, Constants.UI.BuildingStatus.StatusRowBackground),
                buttonSize + buttonBetweenMargin.Y)
            .Add(new IconRow<UpgradeSlot>(model.Upgrades, StatusIconFactories.UpgradeSlot),
                buttonSize + buttonBetweenMargin.Y);
    }

    private sealed class IconRow<T> : CompositeControl
    {
        private readonly IReadonlyBinding<ImmutableArray<T>> source;
        private readonly Func<IReadonlyBinding<T?>, Control> controlFactory;
        private readonly List<Control> iconControls = [];

        public IconRow(
            IReadonlyBinding<ImmutableArray<T>> source,
            Func<IReadonlyBinding<T?>, Control> controlFactory,
            ShapeComponents? background = null)
        {
            this.source = source;
            this.controlFactory = controlFactory;

            if (background is { } components)
            {
                Add(new ComplexBox { Components = components }
                    .Anchor(a => a.Left(margin: Constants.UI.BuildingStatus.StatusRowBackgroundLeftMargin)));
            }

            source.CollectionSize<ImmutableArray<T>, T>().SourceUpdated += updateIconCount;
            updateIconCount(source.Value.Length);
        }

        private void updateIconCount(int newCount)
        {
            var currentCount = iconControls.Count;
            if (currentCount == newCount) return;

            if (currentCount > newCount)
            {
                for (var i = newCount; i < currentCount; i++)
                {
                    Remove(iconControls[i]);
                }

                iconControls.RemoveRange(newCount, currentCount - newCount);
            }
            else
            {
                for (var i = currentCount; i < newCount; i++)
                {
                    var button = iconControl(i);
                    iconControls.Add(button);
                    Add(button);
                }
            }
        }

        private Control iconControl(int i)
        {
            var binding = source.ListElementByIndex<ImmutableArray<T>, T>(i);
            var control = controlFactory(binding);
            control.Anchor(a => a
                .Left(margin: buttonLeftMargin(i), width: buttonSize)
                .Top(relativePercentage: 0.5, margin: -0.5 * buttonSize, height: buttonSize)
            );
            return control;
        }
    }
}
