using Bearded.TD.Game.Weapons;
using Bearded.TD.Rendering;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("instantAim")]
    class InstantAim : Component<Weapon>
    {
        protected override void Initialise()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            Owner.AimDirection.Match(aimIn, () => { });
        }

        private void aimIn(Direction2 direction)
        {
            Owner.Turn(direction - Owner.CurrentDirection);
        }

        public override void Draw(GeometryManager geometries)
        {
        }
    }
}
