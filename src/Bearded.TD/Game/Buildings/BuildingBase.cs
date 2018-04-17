using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.UI.Model;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    abstract class BuildingBase<T> : GameObject, IPositionable, IFactioned, ISelectable
        where T : BuildingBase<T>
    {
        private PositionedFootprint footprint;
        private readonly List<IComponent<T>> components = new List<IComponent<T>>();

        public abstract SelectionState SelectionState { get; }

        public BuildingBlueprint Blueprint { get; }

        protected PositionedFootprint Footprint
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
        
        public abstract void ResetSelection();
        public abstract void Focus(SelectionManager selectionManager);
        public abstract void Select(SelectionManager selectionManager);

        protected virtual void ChangeFootprint(PositionedFootprint newFootprint)
            => Footprint = newFootprint;

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

        protected void DrawTiles(GeometryManager geometries, Color color)
        {
            foreach (var tile in Footprint.OccupiedTiles)
            {
                DrawTile(geometries, color, tile);
            }
        }

        protected void DrawTile(GeometryManager geometries, Color color, Tile<TileInfo> tile)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = color;
            geo.DrawCircle(Game.Level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
        }

        protected void DrawBuildingName(GeometryManager geometries, Color color)
        {
            var geo = geometries.ConsoleFont;
            geo.Color = color;
            geo.Height = .2f;
            geo.SizeCoefficient = new Vector2(1, -1);
            geo.DrawString(Position.NumericValue, Blueprint.Name, .5f, .5f);
        }
    }
}
