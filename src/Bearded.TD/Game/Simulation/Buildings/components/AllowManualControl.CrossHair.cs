using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed partial class AllowManualControl<T>
    {
        private sealed class CrossHair : Component<T>, IManualTarget3, IDrawableComponent
        {
            private readonly IManualTarget2 target;
            private IFactionProvider? faction;
            private SpriteDrawInfo<UVColorVertex, Color> sprite;

            public Position3 Target { get; private set; }
            public bool TriggerPulled { get; private set; }

            public CrossHair(IManualTarget2 target)
            {
                this.target = target;
            }

            protected override void OnAdded()
            {
                faction = (Owner as IComponentOwner).GetComponents<IFactionProvider>().FirstOrDefault();

                var spriteBlueprint = Owner.Game.Meta.Blueprints
                    .Sprites[ModAwareId.ForDefaultMod("particle")]
                    .GetSprite("plus");
                sprite = SpriteDrawInfo.ForUVColor(Owner.Game, null, spriteBlueprint);
            }

            public override void Update(TimeSpan elapsedTime)
            {
                var targetPoint = target.Target;
                var tile = Level.GetTile(targetPoint);
                var targetHeight = Owner.Game.Level.IsValid(tile)
                    ? Owner.Game.GeometryLayer[tile].Geometry.FloorHeight
                    : Unit.Zero;
                var clampedTargetHeight = targetHeight.NumericValue.Clamped(0, 0.25f).U();

                Target = targetPoint.WithZ(clampedTargetHeight + 0.2.U());
                TriggerPulled = target.TriggerDown;
            }

            public void Draw(IComponentDrawer drawer)
            {
                drawer.DrawSprite(
                    sprite,
                    Target.NumericValue,
                    0.5f,
                    45.Degrees().Radians,
                    (faction?.Faction.Color ?? Color.Gray).WithAlpha(0.75f) * 0.75f);
            }
        }
    }
}
