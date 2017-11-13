using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    class BuildingGhost : GameObject, IPositionableGameObject
    {
        private PositionedFootprint footprint;

        private readonly List<Component<BuildingGhost>> components = new List<Component<BuildingGhost>>();

        public Position2 Position => footprint.CenterPosition;

        public BuildingGhost(BuildingBlueprint blueprint)
        {
            components.AddRange(blueprint.ComponentFactories.Select(f => f.CreateForGhost()).NotNull());
        }

        protected override void OnAdded()
        {
            components.ForEach(c => c.OnAdded(this));
        }

        public void SetFootprint(PositionedFootprint footprint)
        {
            this.footprint = footprint;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            foreach (var component in components)
            {
                component.Update(elapsedTime);
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;

            foreach (var tile in footprint.OccupiedTiles)
            {
                if (!tile.IsValid) continue;

                geo.Color = (tile.Info.IsPassable ? Color.Green : Color.Red) * 0.5f;
                geo.DrawCircle(Game.Level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
            }

            foreach (var component in components)
            {
                component.Draw(geometries);
            }
        }
    }
}
