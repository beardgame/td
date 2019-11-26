using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    abstract class BuildingBase<T> : GameObject, IComponentOwner<T>, IFactioned, IPositionable, ISelectable
        where T : BuildingBase<T>
    {
        private PositionedFootprint footprint;

        protected ComponentCollection<T> Components { get; }

        public abstract SelectionState SelectionState { get; }

        public IBuildingBlueprint Blueprint { get; }

        protected PositionedFootprint Footprint
        {
            get => footprint;
            private set
            {
                footprint = value;
                Position = footprint.CenterPosition.WithZ();
                recalculateLocalTransform();
            }
        }

        public Faction Faction { get; }
        public Position3 Position { get; private set; }

        public Matrix2 LocalCoordinateTransform { get; private set; }
        public Angle LocalOrientationTransform => Footprint.Orientation;

        public IEnumerable<Tile> OccupiedTiles => Footprint.OccupiedTiles;

        protected BuildingBase(
            IBuildingBlueprint blueprint,
            Faction faction,
            PositionedFootprint footprint)
        {
            Blueprint = blueprint;
            Faction = faction;
            Footprint = footprint;
            Components = new ComponentCollection<T>((T)this);
        }

        private void recalculateLocalTransform()
        {
            LocalCoordinateTransform = footprint.Orientation.Transformation;
        }

        public abstract void ResetSelection();
        public abstract void Focus(SelectionManager selectionManager);
        public abstract void Select(SelectionManager selectionManager);

        protected virtual void ChangeFootprint(PositionedFootprint newFootprint)
            => Footprint = newFootprint;

        protected override void OnAdded()
        {
            base.OnAdded();

            Components.Add(InitialiseComponents());
        }
        protected abstract IEnumerable<IComponent<T>> InitialiseComponents();

        public IEnumerable<TComponent> GetComponents<TComponent>() where TComponent : IComponent => Components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner<T>.GetComponents<TComponent>() => GetComponents<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => Components.Get<TComponent>();

        public override void Update(TimeSpan elapsedTime)
        {
            Components.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            Components.Draw(geometries);
        }

        protected void DrawTiles(GeometryManager geometries, Color color)
        {
            foreach (var tile in Footprint.OccupiedTiles)
            {
                DrawTile(geometries, color, tile);
            }
        }

        protected void DrawTile(GeometryManager geometries, Color color, Tile tile)
        {
            var geo = geometries.Primitives;
            geo.Color = color;
            geo.DrawCircle(Level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
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
