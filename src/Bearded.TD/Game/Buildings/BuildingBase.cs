using System;
using System.Collections.Generic;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.UI.Model;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    interface IPlacedBuilding : ISelectable
    {
    }

    abstract class PlacedBuildingBase<T> : BuildingBase<T>, IPlacedBuilding
        where T : PlacedBuildingBase<T>
    {
        public SelectionState SelectionState { get; private set; }
        
        protected PlacedBuildingBase(
            BuildingBlueprint blueprint,
            Faction faction,
            PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            DebugAssert.Argument.Satisfies(footprint.IsValid);
        }

        protected override void ChangeFootprint(PositionedFootprint footprint)
            => throw new InvalidOperationException("Cannot change footprint of placed building.");

        public void ResetSelection()
        {
            SelectionState = SelectionState.Default;
        }

        public void Focus(SelectionManager selectionManager)
        {
            if (selectionManager.FocusedObject != this)
                throw new Exception("Cannot focus an object that is not the currently focused object.");
            SelectionState = SelectionState.Focused;
        }

        public void Select(SelectionManager selectionManager)
        {
            if (selectionManager.SelectedObject != this)
                throw new Exception("Cannot select an object that is not the currently selected object.");
            SelectionState = SelectionState.Selected;
        }
        
        protected override void OnAdded()
        {
            base.OnAdded();
            OccupiedTiles.ForEach(tile =>
            {
                var info = tile.Info;
                DebugAssert.State.Satisfies(info.PlacedBuilding == null, "Tile already taken by other placed building.");
                info.PlacedBuilding = this;
            });
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach(tile =>
            {
                var info = tile.Info;
                DebugAssert.State.Satisfies(info.PlacedBuilding == this, "Cannot remove placed building from tile it is not on.");
                info.PlacedBuilding = null;
            });
            base.OnDelete();
        }
    }

    abstract class BuildingBase<T> : GameObject, IPositionable
        where T : BuildingBase<T>
    {
        private PositionedFootprint footprint;
        private readonly List<IComponent<T>> components = new List<IComponent<T>>();

        protected BuildingBlueprint Blueprint { get; }

        public PositionedFootprint Footprint
        {
            get => footprint;
            private set
            {
                footprint = value;
                Position = footprint.CenterPosition;
            }
        }

        public Faction Faction { get; }
        public Position2 Position { get; private set; }
        
        public IEnumerable<Tile<TileInfo>> OccupiedTiles => Footprint.OccupiedTiles;

        protected IReadOnlyCollection<IComponent<T>> Components { get; }

        protected BuildingBase(
            BuildingBlueprint blueprint,
            Faction faction,
            PositionedFootprint footprint)
        {
            Blueprint = blueprint;
            Faction = faction;
            Footprint = footprint;

            Components = components.AsReadOnly();
        }

        protected virtual void ChangeFootprint(PositionedFootprint footprint)
            => Footprint = footprint;

        protected override void OnAdded()
        {
            base.OnAdded();

            InitialiseComponents().ForEach(addComponent);
        }
        protected abstract IEnumerable<IComponent<T>> InitialiseComponents();

        private void addComponent(IComponent<T> component)
        {
            components.Add(component);
            component.OnAdded((T)this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            foreach (var component in components)
                component.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            foreach (var component in Components)
                component.Draw(geometries);
        }
    }
}
