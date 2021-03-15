using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    abstract class BuildingBase<T> : GameObject, IComponentOwner<T>, IFactioned, IPositionable, ISelectable
        where T : BuildingBase<T>
    {
        private PositionedFootprint footprint;

        protected ComponentCollection<T> Components { get; }
        protected ComponentEvents Events { get; } = new();

        public Maybe<IComponentOwner> Parent => Maybe.Nothing;

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
            Components = new ComponentCollection<T>((T)this, Events);
        }

        private void recalculateLocalTransform()
        {
            LocalCoordinateTransform = footprint.Orientation.Transformation;
        }

        public abstract void ResetSelection();
        public abstract void Focus();
        public abstract void Select();

        protected virtual void ChangeFootprint(PositionedFootprint newFootprint)
        {
            Footprint = newFootprint;

            calculatePositionZ();
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            calculatePositionZ();

            Components.Add(InitializeComponents());
        }

        private void calculatePositionZ()
        {
            var z = Game.Level.IsValid(footprint.RootTile)
                ? Game.GeometryLayer[footprint.RootTile].DrawInfo.Height
                : Unit.Zero;

            Position = Position.XY().WithZ(z);
        }

        protected abstract IEnumerable<IComponent<T>> InitializeComponents();
        public IEnumerable<TComponent> GetComponents<TComponent>() => Components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner<T>.GetComponents<TComponent>() => GetComponents<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => Components.Get<TComponent>();

        public override void Update(TimeSpan elapsedTime)
        {
            Components.Update(elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
            Components.Draw(drawers);
        }

        protected void DrawTiles(CoreDrawers drawers, Color color)
        {
            foreach (var tile in Footprint.OccupiedTiles)
            {
                DrawTile(drawers, color, tile);
            }
        }

        protected void DrawTile(CoreDrawers drawers, Color color, Tile tile)
        {
            drawers.Primitives.FillCircle(
                Level.GetPosition(tile).WithZ(Position.Z).NumericValue, Constants.Game.World.HexagonSide, color, 6);
        }

        protected void DrawBuildingName(CoreDrawers drawers, Color color)
        {
            drawers.InGameConsoleFont.DrawLine(
                xyz: Position.NumericValue,
                text: Blueprint.Name,
                parameters: color,
                fontHeight: .2f);
        }
    }
}
