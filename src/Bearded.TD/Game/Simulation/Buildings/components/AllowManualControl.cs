using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class AllowManualControl<T> : Component<T>, IManualControlReport
        where T : IComponentOwner<T>, IGameObject, IPositionable
    {
        private CrossHair? crossHair;
        public ReportType Type => ReportType.ManualControl;

        protected override void OnAdded()
        {
            ReportAggregator.Register(Events, this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
        }

        public Position2 SubjectPosition => Owner.Position.XY();

        public void StartControl(IManualTarget2 target)
        {
            DebugAssert.State.Satisfies(crossHair == null);

            crossHair = new CrossHair(target);
            Owner.AddComponent(crossHair);

            foreach (var turret in (Owner as IComponentOwner).GetComponents<ITurret>())
            {
                turret.OverrideTargeting(crossHair);
            }
        }

        public void EndControl()
        {
            DebugAssert.State.Satisfies(crossHair != null);

            Owner.RemoveComponent(crossHair!);
            crossHair = null;

            foreach (var turret in (Owner as IComponentOwner).GetComponents<ITurret>())
            {
                turret.StopTargetOverride();
            }
        }

        private sealed class CrossHair : Component<T>, IManualTarget3
        {
            private readonly IManualTarget2 target;
            private IFactionProvider? faction;
            private IDrawableSprite<Color> sprite = null!;


            public Position3 Target { get; private set; }
            public bool TriggerDown { get; private set; }

            public CrossHair(IManualTarget2 target)
            {
                this.target = target;
            }

            protected override void OnAdded()
            {
                faction = (Owner as IComponentOwner).GetComponents<IFactionProvider>().FirstOrDefault();

                var sprites = Owner.Game.Meta.Blueprints.Sprites[ModAwareId.ForDefaultMod("particle")];
                sprite = sprites
                    .GetSprite("plus")
                    .MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, UVColorVertex.Create);
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
                TriggerDown = target.TriggerDown;
            }

            public override void Draw(CoreDrawers drawers)
            {
                sprite.Draw(
                    Target.NumericValue,
                    0.5f,
                    45.Degrees().Radians,
                    (faction?.Faction.Color ?? Color.Gray).WithAlpha(0.75f) * 0.75f);
            }
        }
    }
}
