using System.Collections.Generic;
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
    abstract class BuildingBase<T>
        : GameObject,
            IBuilding,
            IComponentOwner<T>,
            IFactioned,
            IPositionable,
            ITransformable
        where T : BuildingBase<T>
    {
        private PositionedFootprint footprint;

        protected ComponentCollection<T> Components { get; }
        protected ComponentEvents Events { get; } = new();

        public Maybe<IComponentOwner> Parent => Maybe.Nothing;
        public abstract IBuildingState State { get; }

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
            Components = new ComponentCollection<T>((T) this, Events);
        }

        private void recalculateLocalTransform()
        {
            LocalCoordinateTransform = footprint.Orientation.Transformation;
        }

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

        protected override void OnDelete()
        {
            Events.Send(new ObjectDeleting());
            base.OnDelete();
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
    }
}
