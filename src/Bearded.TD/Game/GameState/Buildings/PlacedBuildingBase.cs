using System;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.GameState.World;
using Bearded.TD.Game.Meta;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.GameState.Buildings
{
    abstract class PlacedBuildingBase<T> : BuildingBase<T>, IPlacedBuilding
        where T : PlacedBuildingBase<T>
    {
        private SelectionState selectionState;
        public override SelectionState SelectionState => selectionState;

        protected PlacedBuildingBase(
            IBuildingBlueprint blueprint,
            Faction faction,
            PositionedFootprint footprint)
            : base(blueprint, faction, footprint) {}

        protected override void ChangeFootprint(PositionedFootprint footprint)
            => throw new InvalidOperationException("Cannot change footprint of placed building.");

        public override void ResetSelection()
        {
            selectionState = SelectionState.Default;
        }

        public override void Focus(SelectionManager selectionManager)
        {
            selectionManager.CheckCurrentlyFocused(this);
            selectionState = SelectionState.Focused;
        }

        public override void Select(SelectionManager selectionManager)
        {
            selectionManager.CheckCurrentlySelected(this);
            selectionState = SelectionState.Selected;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            DebugAssert.State.Satisfies(Footprint.IsValid(Game.Level));
            Game.BuildingLayer.AddBuilding(this);
        }

        protected override void OnDelete()
        {
            Game.BuildingLayer.RemoveBuilding(this);
            base.OnDelete();
        }
    }
}
