using Bearded.TD.Audio;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Upgrades;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingSounds : Component
{
    protected override void OnAdded() { }

    public override void Activate()
    {
        // We purposefully delay our dependency look-up, since we're not interested in receiving events until we're
        // activated.
        ComponentDependencies.Depend<IUpgradeSlots>(Owner, Events, slots => slots.SlotFilled += onUpgradeSlotFilled);
    }

    private void onUpgradeSlotFilled(int index, IPermanentUpgrade upgrade)
    {
        var sound = upgrade.Element.GetUpgradeSound(Owner.Game.Meta.Blueprints);
        Owner.Game.Meta.SoundScape.PlaySoundAt(sound, Owner.Position);
    }

    public override void Update(TimeSpan elapsedTime) { }
}
