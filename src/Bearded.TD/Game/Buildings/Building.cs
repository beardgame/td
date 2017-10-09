using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.gameplay;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.UI;
using Bearded.TD.Game.World;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    abstract partial class Building : GameObject, IIdable<Building>, ISelectable
    {
        private static readonly Dictionary<SelectionState, Color> drawColors = new Dictionary<SelectionState, Color>
        {
            {SelectionState.Default, Color.Blue},
            {SelectionState.Focused, Color.DarkBlue},
            {SelectionState.Selected, Color.RoyalBlue}
        };

        private readonly BuildingBlueprint blueprint;
        private PositionedFootprint footprint;

        public Id<Building> Id { get; }
        
        public Faction Faction { get; }
        public Position2 Position { get; private set; }
        public int Health { get; private set; }
        private bool isCompleted;
        private double buildProgress;
        public IEnumerable<Tile<TileInfo>> OccupiedTiles => footprint.OccupiedTiles;
        public SelectionState SelectionState { get; private set; }

        private List<Component> components { get; } = new List<Component>();

        public event VoidEventHandler Damaged;

        public WorkerTask WorkerTask
        {
            get
            {
                if (isCompleted)
                    throw new Exception("Cannot create a worker task for a completed building.");
                return new BuildingWorkerTask(this, blueprint);
            }
        }

        protected Building(Id<Building> id, BuildingBlueprint blueprint, PositionedFootprint footprint, Faction faction)
        {
            if (!footprint.IsValid) throw new ArgumentOutOfRangeException();

            Id = id;
            this.blueprint = blueprint;
            this.footprint = footprint;
            Faction = faction;
            Health = 1;
        }

        public void Damage(int damage)
        {
            if (!UserSettings.Instance.Debug.InvulnerableBuildings)
                Health -= damage;
            Damaged?.Invoke();
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);

            Position = footprint.CenterPosition;
            OccupiedTiles.ForEach(tile =>
            {
                Game.Geometry.SetBuilding(tile, this);
                Game.Navigator.AddBackupSink(tile);
            });
        }

        public void ResetToComplete()
        {
            Health = blueprint.MaxHealth;
            if (!isCompleted)
                onCompleted();
        }

        private void onCompleted()
        {
            blueprint.GetComponents().ForEach(components.Add);
            components.ForEach(c => c.OnAdded(this));
            isCompleted = true;
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach(tile =>
            {
                Game.Geometry.SetBuilding(tile, null);
                Game.Navigator.RemoveSink(tile);
            });
        }

        public override void Update(TimeSpan elapsedTime)
        {
            foreach (var component in components)
                component.Update(elapsedTime);

            if (Health <= 0)
            {
                this.Sync(KillBuilding.Command, this);
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            var alpha = isCompleted ? 1 : (float)(buildProgress * 0.9);
            geo.Color = drawColors[SelectionState] * alpha;

            foreach (var tile in footprint.OccupiedTiles)
                geo.DrawCircle(Game.Level.GetPosition(tile).NumericValue, HexagonSide, true, 6);

            foreach (var component in components)
                component.Draw(geometries);
            
            geometries.PointLight.Draw(Position.NumericValue.WithZ(3), 3 + 2 * alpha, Color.Orange * 0.2f);
        }

        public bool HasComponentOfType<T>()
        {
            return components.OfType<T>().FirstOrDefault() != null;
        }

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
    }
}
