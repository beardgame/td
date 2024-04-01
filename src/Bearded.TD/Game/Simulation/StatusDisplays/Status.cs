using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed class Status(StatusSpec spec, StatusAppearance appearance)
{
    private Instant? expiry;

    public StatusSpec Spec => spec;

    public StatusAppearance Appearance
    {
        get => appearance;
        set
        {
            if (appearance == value) return;
            appearance = value;
            AppearanceChanged?.Invoke();
        }
    }

    public Instant? Expiry
    {
        get => expiry;
        set
        {
            if (expiry == value) return;
            expiry = value;
            ExpiryChanged?.Invoke();
        }
    }

    public event VoidEventHandler? AppearanceChanged;
    public event VoidEventHandler? ExpiryChanged;
}
