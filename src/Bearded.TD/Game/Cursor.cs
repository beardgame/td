using System;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game
{
    class Cursor : GameObject
    {
        private readonly Func<Position2> positionRetriever;

        public Position2 Position => positionRetriever();

        public Cursor(Func<Position2> positionRetriever)
        {
            this.positionRetriever = positionRetriever;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            Game.RegisterSingleton(this);
        }

        public override void Update(TimeSpan elapsedTime) { }
        public override void Draw(GeometryManager geometries)
        {
            geometries.PointLight.Draw(Position.NumericValue.WithZ(3), 5, Color.Pink);
        }
    }
}
