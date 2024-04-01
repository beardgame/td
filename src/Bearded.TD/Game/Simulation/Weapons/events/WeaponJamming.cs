using Bearded.TD.Commands;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("weaponJamming")]
sealed class WeaponJamming : Component<WeaponJamming.IParameters>, IPreviewListener<PreviewFireWeapon>, IWeaponJammer
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(0.02)]
        public double ProbabilityPerShot { get; }

        [Modifiable(4)]
        public TimeSpan Duration { get; }
    }

    private ActiveJam? activeJam;
    private IStatusTracker? statusDisplay;
    private ISpriteBlueprint? sprite;

    public WeaponJamming(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate()
    {
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
        base.OnRemoved();
    }

    public void Jam(TimeSpan duration)
    {
        var newJam = new ActiveJam(Owner.Game.Time, duration);

        if (activeJam is { } oldJam && newJam.End <= oldJam.End)
        {
            return;
        }

        if (activeJam is null &&
            (statusDisplay is not null || Owner.TryGetSingleComponentInOwnerTree(out statusDisplay)))
        {
            sprite ??= Owner.Game.Meta.Blueprints.LoadStatusIconSprite("spanner");
            statusDisplay.AddStatus(
                new StatusSpec(
                    StatusType.Negative,
                    StatusDrawSpec.StaticIcon("spanner".ToStatusIconSpriteId()),
                    null,
                    IconStatusDrawer.FromSpriteBlueprint(Owner.Game, sprite)),
                newJam.End);
        }

        activeJam = newJam;
    }

    public void PreviewEvent(ref PreviewFireWeapon @event)
    {
        Owner.Sync(maybeJamWeapon);

        if (activeJam is { } jam && jam.End > Owner.Game.Time)
        {
            @event = @event.Cancelled();
        }
    }

    private void maybeJamWeapon(ICommandDispatcher<GameInstance> dispatcher)
    {
        var shouldJam = activeJam is null && StaticRandom.Bool(Parameters.ProbabilityPerShot);
        if (!shouldJam) return;
        dispatcher.Dispatch(JamWeapon.Command(Owner, Parameters.Duration));
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (activeJam is { } jam && jam.End <= Owner.Game.Time)
        {
            activeJam = null;
        }
    }

    private readonly record struct ActiveJam(Instant Start, TimeSpan Duration)
    {
        public Instant End => Start + Duration;
    }
}
