using System;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Buildings
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
            : base(blueprint, faction, footprint)
        {
            DebugAssert.Argument.Satisfies(footprint.IsValid);
        }

        protected override void ChangeFootprint(PositionedFootprint footprint)
            => throw new InvalidOperationException("Cannot change footprint of placed building.");

        public override void ResetSelection()
        {
            selectionState = SelectionState.Default;
        }

        public override void Focus(SelectionManager selectionManager)
        {
            if (selectionManager.FocusedObject != this)
                throw new Exception("Cannot focus an object that is not the currently focused object.");
            selectionState = SelectionState.Focused;
        }

        public override void Select(SelectionManager selectionManager)
        {
            if (selectionManager.SelectedObject != this)
                throw new Exception("Cannot select an object that is not the currently selected object.");
            selectionState = SelectionState.Selected;
        }
        
        protected override void OnAdded()
        {
            base.OnAdded();
            Game.BuildingLayer.AddBuilding(this);
        }

        protected override void OnDelete()
        {
            Game.BuildingLayer.RemoveBuilding(this);
            base.OnDelete();
        }
    }
}
