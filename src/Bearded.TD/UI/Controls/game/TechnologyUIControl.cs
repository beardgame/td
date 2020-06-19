using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Technologies;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenToolkit.Mathematics;

namespace Bearded.TD.UI.Controls
{
    sealed class TechnologyUIControl : CompositeControl
    {
        private readonly TechnologyUI model;

        private readonly ListControl technologyList = new ListControl(new ViewportClippingLayerControl());
        private readonly TechnologyDetailsControl technologyDetails;

        public event VoidEventHandler? CloseButtonClicked;

        public TechnologyUIControl(TechnologyUI model)
        {
            this.model = model;

            technologyDetails = new TechnologyDetailsControl(model.Model);

            Add(new BackgroundBox());
            Add(new Label {FontSize = 36, Text = "Research"}.Anchor(a => a.Top(margin: 8, height: 40)));
            Add(LegacyDefault.Button("close", 16)
                .Anchor(a => a.Top(margin: 16, height: 24).Right(margin: 16, width: 92))
                .Subscribe(btn => btn.Clicked += () => CloseButtonClicked?.Invoke()));

            Add(technologyList.Anchor(a =>
                a.Top(margin: 56).Bottom(margin: 16).Left(margin: 16, width: 300)));
            Add(technologyDetails.Anchor(a =>
                a.Top(margin: 56).Bottom(margin: 16).Left(margin: 332).Right(margin: 16)));

            model.TechnologiesUpdated += updateTechnologiesList;

            updateTechnologiesList();
        }

        private void updateTechnologiesList()
        {
            technologyList.ItemSource = new TechnologyListItemSource(
                model.Model, tech => technologyDetails.SetTechnologyToDisplay(Maybe.Just(tech)));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private sealed class TechnologyListItemSource : IListItemSource
        {
            private readonly TechnologyUIModel model;
            private readonly Action<ITechnologyBlueprint> buttonClickCallback;
            private readonly ImmutableList<ITechnologyBlueprint> sortedTechnologies;

            public int ItemCount { get; }

            public TechnologyListItemSource(TechnologyUIModel model, Action<ITechnologyBlueprint> buttonClickCallback)
            {
                this.model = model;
                this.buttonClickCallback = buttonClickCallback;

                sortedTechnologies = model.Technologies.Sort((left, right) =>
                {
                    var leftIsLockedAsInt =
                        model.StatusFor(left) == TechnologyUIModel.TechnologyStatus.Unlocked ? 0 : 1;
                    var rightIsLockedAsInt =
                        model.StatusFor(right) == TechnologyUIModel.TechnologyStatus.Unlocked ? 0 : 1;

                    return leftIsLockedAsInt == rightIsLockedAsInt
                        ? string.Compare(left.Name, right.Name, StringComparison.Ordinal)
                        : leftIsLockedAsInt.CompareTo(rightIsLockedAsInt);
                });

                ItemCount = model.Technologies.Count;
            }

            public double HeightOfItemAt(int index) => 32;

            public Control CreateItemControlFor(int index)
            {
                var technology = sortedTechnologies[index];
                var button = new TechnologyButton(
                    technology,
                    () => model.StatusFor(technology));
                button.Clicked += () => buttonClickCallback(technology);
                return button;
            }

            public void DestroyItemControlAt(int index, Control control)
            {
            }
        }

        private sealed class TechnologyButton : Button
        {
            private readonly Func<TechnologyUIModel.TechnologyStatus> statusGetter;
            private readonly BackgroundBox backgroundBox;

            public TechnologyButton(ITechnologyBlueprint technology,
                Func<TechnologyUIModel.TechnologyStatus> statusGetter)
            {
                this.statusGetter = statusGetter;

                this.WithDefaultStyle(technology.Name, fontSize: 20);
                backgroundBox = new BackgroundBox();
                Add(backgroundBox);
            }

            public override void Render(IRendererRouter r)
            {
                backgroundBox.Color = .25f * colorForStatus(statusGetter());
                base.Render(r);
            }
        }

        private sealed class TechnologyDetailsControl : CompositeControl
        {
            private readonly TechnologyUIModel model;

            private readonly Label headerLabel = new Label
            {
                FontSize = 32, TextAnchor = Label.TextAnchorLeft
            };

            private readonly Label costLabel = new Label
            {
                Color = Constants.Game.GameUI.TechPointsColor, FontSize = 18, TextAnchor = Label.TextAnchorLeft
            };

            private readonly Label unlockButtonLabel = new Label {FontSize = 16};
            private readonly Button unlockButton;

            private readonly ListControl unlocksList;
            private readonly ListControl dependenciesList;

            private Maybe<ITechnologyBlueprint> technology;

            public TechnologyDetailsControl(TechnologyUIModel model)
            {
                this.model = model;

                Add(headerLabel.Anchor(a => a.Top(height: 40).Right(margin: 208)));
                Add(costLabel.Anchor(a => a.Top(margin: 48, height: 24)));
                Add(new Label("Unlocks:") {FontSize = 24, TextAnchor = Label.TextAnchorLeft}
                    .Anchor(a => a.Top(margin: 80, height: 32)));
                Add(new Label("Required technologies:") {FontSize = 24, TextAnchor = Label.TextAnchorLeft}
                    .Anchor(a => a.Bottom(margin: 128, height: 32)));

                unlockButton = new Button().WithDefaultStyle(unlockButtonLabel);
                Add(unlockButton.Anchor(a => a.Top(height: 32, margin: 4).Right(margin: 8, width: 200)));
                unlockButton.Clicked += onUnlockButtonClicked;

                unlocksList = new ListControl().Anchor(a => a.Top(margin: 120).Bottom(margin: 180));
                Add(unlocksList);
                dependenciesList = new ListControl().Anchor(a => a.Bottom(height: 120));
                Add(dependenciesList);

                SetTechnologyToDisplay(Maybe.Nothing);
            }

            private void onUnlockButtonClicked()
            {
                var tech = technology.ValueOrDefault(null);
                var techStatus = model.StatusFor(tech);

                switch (techStatus)
                {
                    case TechnologyUIModel.TechnologyStatus.Unlocked:
                        break;
                    case TechnologyUIModel.TechnologyStatus.Queued:
                        model.ClearTechnologyQueue();
                        break;
                    case TechnologyUIModel.TechnologyStatus.CanBeUnlocked:
                    case TechnologyUIModel.TechnologyStatus.MissingResources:
                    case TechnologyUIModel.TechnologyStatus.MissingDependencies:
                        model.ReplaceTechnologyQueue(tech);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public void SetTechnologyToDisplay(Maybe<ITechnologyBlueprint> technologyToDisplay)
            {
                technology = technologyToDisplay;
                technology.Match(
                    onValue: tech =>
                    {
                        headerLabel.Text = tech.Name;
                        costLabel.Text = $"{tech.Cost} tech points";
                        unlockButton.IsVisible = true;
                        unlocksList.ItemSource =
                            new TechnologyUnlocksListItemSource(tech.Unlocks, model.DependentsFor(tech));
                        dependenciesList.ItemSource =
                            new TechnologyDependenciesListItemSource(tech.RequiredTechs, model.StatusFor);
                    },
                    onNothing: () =>
                    {
                        headerLabel.Text = "Select a technology from the list";
                        costLabel.Text = "";
                        unlockButton.IsVisible = false;
                        unlocksList.ItemSource =
                            new TechnologyUnlocksListItemSource(
                                Enumerable.Empty<ITechnologyUnlock>(), Enumerable.Empty<ITechnologyBlueprint>());
                        dependenciesList.ItemSource =
                            new TechnologyDependenciesListItemSource(
                                Enumerable.Empty<ITechnologyBlueprint>(), model.StatusFor);
                    });
            }

            public override void Render(IRendererRouter r)
            {
                technology.Match(updateForTechnology, updateForEmpty);
                base.Render(r);
            }

            private void updateForTechnology(ITechnologyBlueprint tech)
            {
                var technologyStatus = model.StatusFor(tech);

                switch (technologyStatus)
                {
                    case TechnologyUIModel.TechnologyStatus.Unlocked:
                        unlockButtonLabel.Text = "Unlocked";
                        break;
                    case TechnologyUIModel.TechnologyStatus.Queued:
                        unlockButtonLabel.Text = $"Queued ({model.QueuePositionFor(tech)})";
                        break;
                    case TechnologyUIModel.TechnologyStatus.CanBeUnlocked:
                        unlockButtonLabel.Text = "Unlock";
                        break;
                    case TechnologyUIModel.TechnologyStatus.MissingResources:
                    case TechnologyUIModel.TechnologyStatus.MissingDependencies:
                        unlockButtonLabel.Text = "Queue";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private static void updateForEmpty() {}

            protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

            private sealed class TechnologyUnlocksListItemSource : IListItemSource
            {
                private readonly ImmutableList<ITechnologyUnlock> unlocks;
                private readonly ImmutableList<ITechnologyBlueprint> dependents;

                public int ItemCount => unlocks.Count + dependents.Count;

                public TechnologyUnlocksListItemSource(
                    IEnumerable<ITechnologyUnlock> unlocks,
                    IEnumerable<ITechnologyBlueprint> dependents)
                {
                    this.unlocks = ImmutableList.CreateRange(unlocks);
                    this.dependents = ImmutableList.CreateRange(dependents);
                }

                public double HeightOfItemAt(int index) => 24;

                public Control CreateItemControlFor(int index) =>
                    new Label(labelFor(index)) {FontSize = 20, TextAnchor = new Vector2d(0, .5)};

                private string labelFor(int index) =>
                    index < unlocks.Count
                        ? $"- {unlocks[index].Description}"
                        : $"- Required technology for: {dependents[index - unlocks.Count].Name}";

                public void DestroyItemControlAt(int index, Control control) {}
            }

            private sealed class TechnologyDependenciesListItemSource : IListItemSource
            {
                private readonly ImmutableList<ITechnologyBlueprint> dependencies;
                private readonly Func<ITechnologyBlueprint, TechnologyUIModel.TechnologyStatus> statusGetter;

                public int ItemCount => dependencies.Count;

                public TechnologyDependenciesListItemSource(
                    IEnumerable<ITechnologyBlueprint> dependencies,
                    Func<ITechnologyBlueprint, TechnologyUIModel.TechnologyStatus> statusGetter)
                {
                    this.dependencies = ImmutableList.CreateRange(dependencies);
                    this.statusGetter = statusGetter;
                }

                public double HeightOfItemAt(int index) => 24;

                public Control CreateItemControlFor(int index) =>
                    new DynamicLabel(() => labelFor(index), () => colorFor(index))
                    {
                        FontSize = 20, TextAnchor = new Vector2d(0, .5)
                    };

                private string labelFor(int index) => $"- {dependencies[index].Name}";

                private Color colorFor(int index) => colorForStatus(statusGetter(dependencies[index]));

                public void DestroyItemControlAt(int index, Control control) {}
            }
        }

        private static Color colorForStatus(TechnologyUIModel.TechnologyStatus status)
        {
            switch (status)
            {
                case TechnologyUIModel.TechnologyStatus.Unlocked:
                    return Color.Green;
                case TechnologyUIModel.TechnologyStatus.Queued:
                    return Color.Aqua;
                case TechnologyUIModel.TechnologyStatus.CanBeUnlocked:
                    return Color.Yellow;
                case TechnologyUIModel.TechnologyStatus.MissingResources:
                    return Color.Red;
                case TechnologyUIModel.TechnologyStatus.MissingDependencies:
                    return Color.Purple;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
