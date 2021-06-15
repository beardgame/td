using System;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings
{
    abstract class PlacedBuildingBase<T> : BuildingBase<T>, IPlacedBuilding
        where T : PlacedBuildingBase<T>
    {
        protected BuildingState MutableState { get; }
        public override IBuildingState State { get; }

        protected PlacedBuildingBase(
            IBuildingBlueprint blueprint,
            Faction faction,
            PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            MutableState = new BuildingState();
            State = MutableState.CreateProxy();
        }

        protected override void ChangeFootprint(PositionedFootprint footprint)
            => throw new InvalidOperationException("Cannot change footprint of placed building.");

        public void ResetSelection()
        {
            MutableState.SelectionState = SelectionState.Default;
        }

        public void Focus()
        {
            MutableState.SelectionState = SelectionState.Focused;
        }

        public void Select()
        {
            MutableState.SelectionState = SelectionState.Selected;
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
