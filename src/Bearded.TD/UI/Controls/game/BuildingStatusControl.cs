using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.UI.BuildingStatus;

namespace Bearded.TD.UI.Controls;

using Veterancy = Constants.UI.BuildingStatus.Veterancy;

sealed class BuildingStatusControl : CompositeControl
{
    private static readonly Vector2d buttonBetweenMargin = (Constants.UI.Button.Margin, Constants.UI.Button.Margin * 3);
    private static double buttonLeftMargin(int i) => i * (ButtonSize + buttonBetweenMargin.X);

    public Vector2d Size { get; }

    public BuildingStatusControl(BuildingStatus model)
    {
        // TODO: UI library doesn't allow for this to apply to all nested elements, which is really what we need...
        this.BindIsClickThrough(model.ShowExpanded.Negate());
        Add(new ComplexBox
        {
            Components = Background,
            CornerRadius = 5,
        });

        var innerContainer = new CompositeControl();
        Add(innerContainer.Anchor(a => a.MarginAllSides(Padding).Top()));

        var column = innerContainer.BuildFixedColumn();
        column
            .AddHeader(model.ShowExpanded.Transform(b => b ? "Expanded" : "Preview"))
            .Add(new VeterancyRow(model.Veterancy), Veterancy.RowHeight)
            .Add(new IconRow<Status>(
                    model.Statuses, StatusIconFactories.StatusIcon, StatusRowBackground),
                ButtonSize + buttonBetweenMargin.Y)
            .Add(new IconRow<UpgradeSlot>(model.Upgrades, StatusIconFactories.UpgradeSlot),
                ButtonSize + buttonBetweenMargin.Y);

        Size = (300, column.Height + Padding);
    }

    private sealed class VeterancyRow : CompositeControl
    {
        private readonly Label levelLabel;

        private readonly GradientStop[] experienceBarGradient =
        [
            new GradientStop(0, Veterancy.ExperienceColor),
            new GradientStop(0, Veterancy.ExperienceColor),
            new GradientStop(0, Color.Transparent),
            new GradientStop(1, Color.Transparent),
        ];

        public VeterancyRow(IReadonlyBinding<VeterancyStatus> veterancy)
        {
            var iconMargin = 0.5 * (Veterancy.RowHeight - Veterancy.LevelIconSize);
            var barTopMargin = 0.5 * (Veterancy.RowHeight - Veterancy.ExperienceBarHeight);
            var barLeftMargin = Veterancy.LevelIconSize + iconMargin;

            levelLabel = new Label
            {
                Color = Veterancy.ExperienceColor,
                FontSize = Veterancy.LevelIconSize,
            };

            var experienceBar = new ComplexBox
            {
                CornerRadius = Veterancy.ExperienceBarCornerRadius,
                Components = Veterancy.ExperienceBarColors with
                {
                    Fill = ShapeColor.FromMutable(experienceBarGradient, GradientDefinition.Linear(
                        AnchorPoint.Relative((0, 0)),
                        AnchorPoint.Relative((1, 0))
                    )),
                },
            };

            this.Add([
                levelLabel.Anchor(a => a
                    .Left(width: Veterancy.LevelIconSize)
                    .Top(height: Veterancy.LevelIconSize, margin: iconMargin)),
                experienceBar.Anchor(a => a
                    .Left(margin: barLeftMargin)
                    .Top(margin: barTopMargin, height: Veterancy.ExperienceBarHeight)
                    .Right(relativePercentage: 0.75)),
            ]);

            update(veterancy.Value);
            veterancy.SourceUpdated += update;
        }

        private void update(VeterancyStatus t)
        {
            levelLabel.Text = t.Level.ToString();
            experienceBarGradient[1] = (t.PercentageToNextLevel, Veterancy.ExperienceColor);
            experienceBarGradient[2] = (t.PercentageToNextLevel, Color.Transparent);
        }
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
                    .Anchor(a => a.Left(margin: StatusRowBackgroundLeftMargin)));
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
                .Left(margin: buttonLeftMargin(i), width: ButtonSize)
                .Top(relativePercentage: 0.5, margin: -0.5 * ButtonSize, height: ButtonSize)
            );
            return control;
        }
    }
}
