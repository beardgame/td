using System;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Technologies;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.UI.Controls
{
    sealed class TechnologyUIControl : CompositeControl
    {
        private readonly TechnologyUI model;

        private readonly ListControl technologyList = new ListControl(new ViewportClippingLayerControl());
        private readonly TechnologyDetailsControl technologyDetails;

        public TechnologyUIControl(TechnologyUI model)
        {
            this.model = model;

            technologyDetails = new TechnologyDetailsControl(model.Game);

            Add(new BackgroundBox());
            Add(new Label { FontSize = 36, Text = "Research"}.Anchor(a => a.Top(margin: 8, height: 40)));
            Add(Default.Button("close", 16)
                .Anchor(a => a.Top(margin: 16, height: 24).Right(margin: 16, width: 92))
                .Subscribe(btn => btn.Clicked += model.OnCloseClicked));

            Add(technologyList.Anchor(a =>
                a.Top(margin: 56).Bottom(margin: 16).Left(margin: 16).Right(relativePercentage: .33, margin: 8)));
            Add(technologyDetails.Anchor(a =>
                a.Top(margin: 56).Bottom(margin: 16).Left(relativePercentage: .33, margin: 8).Right(margin: 16)));

            model.TechnologiesUpdated += updateTechnologiesList;

            updateTechnologiesList();
        }

        private void updateTechnologiesList()
        {
            technologyList.ItemSource = new TechnologyListItemSource(
                model.Game, tech => technologyDetails.SetTechnologyToDisplay(Maybe.Just(tech)));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private sealed class TechnologyListItemSource : IListItemSource
        {
            private readonly Action<Technology> buttonClickCallback;
            public int ItemCount { get; }

            private readonly ImmutableList<Technology> lockedTechnologies;
            private readonly ImmutableList<Technology> unlockedTechnologies;

            public TechnologyListItemSource(GameInstance game, Action<Technology> buttonClickCallback)
            {
                this.buttonClickCallback = buttonClickCallback;
                ItemCount = game.Blueprints.Technologies.Count;

                var lookup =
                    game.Blueprints.Technologies.Values.ToLookup(game.Me.Faction.Technology.IsTechnologyLocked);
                lockedTechnologies = lookup[true].ToImmutableList();
                unlockedTechnologies = lookup[false].ToImmutableList();
            }

            public double HeightOfItemAt(int index) => 32;

            public Control CreateItemControlFor(int index)
            {
                var technology = getTechnologyFor(index);
                var button = new TechnologyButton(technology, index < lockedTechnologies.Count);
                button.Clicked += () => buttonClickCallback(technology);
                return button;
            }

            private Technology getTechnologyFor(int index) =>
                index < lockedTechnologies.Count
                    ? lockedTechnologies[index]
                    : unlockedTechnologies[index - lockedTechnologies.Count];

            public void DestroyItemControlAt(int index, Control control) {}
        }

        private sealed class TechnologyButton : Button
        {
            public TechnologyButton(Technology technology, bool isLocked)
            {
                this.WithDefaultStyle(technology.Name);
                Add(new BackgroundBox{Color= .25f * (isLocked ? Color.Red : Color.Green)});
            }
        }

        private sealed class TechnologyDetailsControl : CompositeControl
        {
            private readonly GameInstance game;

            private readonly Label headerLabel = new Label {FontSize = 32, TextAnchor = new Vector2d(0, .5)};

            private readonly Label unlockButtonLabel = new Label {FontSize = 16};
            private readonly Button unlockButton;

            private Maybe<Technology> technology;

            public TechnologyDetailsControl(GameInstance game)
            {
                this.game = game;

                Add(headerLabel.Anchor(a => a.Top(height: 40).Right(margin: 208)));

                unlockButton = new Button().WithDefaultStyle(unlockButtonLabel);
                Add(unlockButton.Anchor(a => a.Top(height: 32, margin: 4).Right(margin: 8, width: 200)));
                unlockButton.Clicked += () =>
                    game.Request(UnlockTechnology.Request(game.Me.Faction, technology.ValueOrDefault(null)));

                SetTechnologyToDisplay(Maybe.Nothing);
            }

            public void SetTechnologyToDisplay(Maybe<Technology> technologyToDisplay)
            {
                technology = technologyToDisplay;
                technology.Match(
                    onValue: tech =>
                    {
                        headerLabel.Text = tech.Name;
                        unlockButton.IsVisible = true;
                    },
                    onNothing: () =>
                    {
                        headerLabel.Text = "Select a technology from the list";
                        unlockButton.IsVisible = false;
                    });
            }

            public override void Render(IRendererRouter r)
            {
                technology.Match(updateForTechnology, updateForEmpty);
                base.Render(r);
            }

            private void updateForTechnology(Technology tech)
            {
                var myFactionTechManager = game.Me.Faction.Technology;
                var isTechLocked = myFactionTechManager.IsTechnologyLocked(tech);
                unlockButton.IsEnabled = myFactionTechManager.TechPoints >= tech.Cost && isTechLocked;
                unlockButtonLabel.Text = isTechLocked ? $"Unlock for {tech.Cost} smarts" : "Unlocked";
            }

            private void updateForEmpty() {}

            protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
        }
    }
}
