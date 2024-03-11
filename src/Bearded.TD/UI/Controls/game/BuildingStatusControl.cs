using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatusControl : CompositeControl
{
    public static readonly Vector2d Size = (200, headerSize + 2 * (buttonSize + buttonBetweenMargin));

    public BuildingStatusControl(BuildingStatus model)
    {
        // TODO: UI library doesn't allow for this to apply to all nested elements, which is really what we need...
        this.BindIsClickThrough(model.ShowExpanded.Negate());
        Add(new BackgroundBox());

        var column = this.BuildFixedColumn();
        column
            .AddHeader(model.ShowExpanded.Transform(b => b ? "Expanded" : "Preview"))
            .Add(new IconRow<Status>(model.Statuses, StatusIconFactories.StatusIcon), buttonSize + buttonBetweenMargin)
            .Add(new IconRow<UpgradeSlot>(model.Upgrades, StatusIconFactories.UpgradeSlot), buttonSize + buttonBetweenMargin);
    }

    private sealed class IconRow<T> : CompositeControl
    {
        private readonly IReadonlyBinding<ImmutableArray<T>> source;
        private readonly Func<IReadonlyBinding<T?>, Control> controlFactory;
        private readonly List<Control> iconControls = [];

        public IconRow(IReadonlyBinding<ImmutableArray<T>> source, Func<IReadonlyBinding<T?>, Control> controlFactory)
        {
            this.source = source;
            this.controlFactory = controlFactory;

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

    private const double headerSize = Constants.UI.Text.HeaderLineHeight;
    private const double buttonBetweenMargin = Constants.UI.Button.Margin;
    private const double buttonSize = Constants.UI.Button.SquareButtonSize;
    private static double buttonLeftMargin(int i) => i * (buttonSize + buttonBetweenMargin);
}
