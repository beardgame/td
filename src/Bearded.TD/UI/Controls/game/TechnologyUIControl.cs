using System;
using System.Collections.Generic;
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
        private enum TechnologyState
        {
            Locked,
            Queued,
            Unlocked
        }

        private readonly TechnologyUI model;

        private readonly ListControl technologyList = new ListControl(new ViewportClippingLayerControl());
        private readonly TechnologyDetailsControl technologyDetails;

        public event VoidEventHandler CloseButtonClicked;

        public TechnologyUIControl(TechnologyUI model)
        {
            this.model = model;

            technologyDetails = new TechnologyDetailsControl(model.Game);

            Add(new BackgroundBox());
            Add(new Label { FontSize = 36, Text = "Research"}.Anchor(a => a.Top(margin: 8, height: 40)));
            Add(Default.Button("close", 16)
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
                model.Game, tech => technologyDetails.SetTechnologyToDisplay(Maybe.Just(tech)));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private sealed class TechnologyListItemSource : IListItemSource
        {
            private readonly Action<ITechnologyBlueprint> buttonClickCallback;
            public int ItemCount { get; }

            private readonly TechnologyManager factionTechnology;
            private readonly ImmutableList<ITechnologyBlueprint> lockedTechnologies;
            private readonly ImmutableList<ITechnologyBlueprint> unlockedTechnologies;
            private readonly ImmutableHashSet<ITechnologyBlueprint> queuedTechnologies;

            public TechnologyListItemSource(GameInstance game, Action<ITechnologyBlueprint> buttonClickCallback)
            {
                this.buttonClickCallback = buttonClickCallback;
                var asList = game.Blueprints.Technologies.All.ToList();
                ItemCount = asList.Count;

                factionTechnology = game.Me.Faction.Technology;
                var lookup = asList.ToLookup(factionTechnology.IsTechnologyLocked);
                lockedTechnologies = lookup[true].OrderBy(t => t.Name).ToImmutableList();
                unlockedTechnologies = lookup[false].OrderBy(t => t.Name).ToImmutableList();
                queuedTechnologies =
                    lockedTechnologies.Where(factionTechnology.IsTechnologyQueued).ToImmutableHashSet();
            }

            public double HeightOfItemAt(int index) => 32;

            public Control CreateItemControlFor(int index)
            {
                var technology = getTechnologyFor(index);
                var button = new TechnologyButton(
                    technology,
                    stateFor(index),
                    () => technology.Cost <= factionTechnology.TechPoints);
                button.Clicked += () => buttonClickCallback(technology);
                return button;
            }

            private TechnologyState stateFor(int index)
            {
                if (index >= lockedTechnologies.Count)
                {
                    return TechnologyState.Unlocked;
                }
                return queuedTechnologies.Contains(getTechnologyFor(index))
                    ? TechnologyState.Queued
                    : TechnologyState.Locked;
            }

            private ITechnologyBlueprint getTechnologyFor(int index) =>
                index < lockedTechnologies.Count
                    ? lockedTechnologies[index]
                    : unlockedTechnologies[index - lockedTechnologies.Count];

            public void DestroyItemControlAt(int index, Control control) {}
        }

        private sealed class TechnologyButton : Button
        {
            private readonly TechnologyState state;
            private readonly Func<bool> canBeUnlocked;
            private readonly BackgroundBox backgroundBox;

            public TechnologyButton(ITechnologyBlueprint technology, TechnologyState state, Func<bool> canBeUnlocked)
            {
                this.state = state;
                this.canBeUnlocked = canBeUnlocked;

                this.WithDefaultStyle(technology.Name, fontSize: 20);
                backgroundBox = new BackgroundBox();
                Add(backgroundBox);
            }

            public override void Render(IRendererRouter r)
            {
                switch (state)
                {
                    case TechnologyState.Locked:
                        if (canBeUnlocked())
                        {
                            backgroundBox.Color = .25f * Color.Yellow;
                        }
                        else
                        {
                            backgroundBox.Color = .25f * Color.Red;
                        }
                        break;
                    case TechnologyState.Queued:
                        backgroundBox.Color = .25f * Color.Aqua;
                        break;
                    case TechnologyState.Unlocked:
                        backgroundBox.Color = .25f * Color.Green;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                base.Render(r);
            }
        }

        private sealed class TechnologyDetailsControl : CompositeControl
        {
            private readonly GameInstance game;

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

            private Maybe<ITechnologyBlueprint> technology;

            public TechnologyDetailsControl(GameInstance game)
            {
                this.game = game;

                Add(headerLabel.Anchor(a => a.Top(height: 40).Right(margin: 208)));
                Add(costLabel.Anchor(a => a.Top(margin: 48, height: 24)));
                Add(new Label("Unlocks:") { FontSize = 24, TextAnchor = Label.TextAnchorLeft }
                    .Anchor(a => a.Top(margin: 80, height: 32)));

                unlockButton = new Button().WithDefaultStyle(unlockButtonLabel);
                Add(unlockButton.Anchor(a => a.Top(height: 32, margin: 4).Right(margin: 8, width: 200)));
                unlockButton.Clicked += onUnlockButtonClicked;

                unlocksList = new ListControl().Anchor(a => a.Top(margin: 120));
                Add(unlocksList);

                SetTechnologyToDisplay(Maybe.Nothing);
            }

            private void onUnlockButtonClicked()
            {
                var tech = technology.ValueOrDefault(null);
                var factionTechnology = game.Me.Faction.Technology;

                if (factionTechnology.IsTechnologyQueued(tech))
                {
                    game.Meta.Logger.Debug?.Log("dequeue");
                    game.Request(DequeueTechnology.Request(game.Me.Faction, tech));
                }
                else
                {
                    game.Request(factionTechnology.TechPoints >= tech.Cost
                        ? UnlockTechnology.Request(game.Me.Faction, tech)
                        : QueueTechnology.Request(game.Me.Faction, tech));
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
                        unlocksList.ItemSource = new TechnologyUnlocksListItemSource(tech.Unlocks);
                    },
                    onNothing: () =>
                    {
                        headerLabel.Text = "Select a technology from the list";
                        costLabel.Text = "";
                        unlockButton.IsVisible = false;
                        unlocksList.ItemSource =
                            new TechnologyUnlocksListItemSource(Enumerable.Empty<ITechnologyUnlock>());
                    });
            }

            public override void Render(IRendererRouter r)
            {
                technology.Match(updateForTechnology, updateForEmpty);
                base.Render(r);
            }

            private void updateForTechnology(ITechnologyBlueprint tech)
            {
                var myFactionTechManager = game.Me.Faction.Technology;
                var isTechLocked = myFactionTechManager.IsTechnologyLocked(tech);
                if (!isTechLocked)
                {
                    unlockButtonLabel.Text = "Unlocked";
                }
                else
                {
                    if (myFactionTechManager.IsTechnologyQueued(tech))
                    {
                        unlockButtonLabel.Text = $"Queued ({myFactionTechManager.QueuePositionFor(tech)})";
                    }
                    else
                    {
                        unlockButtonLabel.Text = myFactionTechManager.TechPoints >= tech.Cost ? "Unlock" : "Queue";
                    }
                }
            }

            private static void updateForEmpty() {}

            protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

            private sealed class TechnologyUnlocksListItemSource : IListItemSource
            {
                private readonly ImmutableList<ITechnologyUnlock> unlocks;

                public int ItemCount => unlocks.Count;

                public TechnologyUnlocksListItemSource(IEnumerable<ITechnologyUnlock> unlocks)
                {
                    this.unlocks = ImmutableList.CreateRange(unlocks);
                }

                public double HeightOfItemAt(int index) => 24;

                public Control CreateItemControlFor(int index) =>
                    new Label($"- {unlocks[index].Description}") { FontSize = 20, TextAnchor = new Vector2d(0, .5) };

                public void DestroyItemControlAt(int index, Control control) {}
            }
        }
    }
}
