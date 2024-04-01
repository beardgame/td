using System;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Controls;

sealed class ObservableStatus : IDisposable
{
    private readonly Status status;
    private readonly Binding<StatusAppearance> appearance;
    private readonly Binding<Instant?> expiry;

    public StatusSpec Spec => status.Spec;
    public IReadonlyBinding<StatusAppearance> Appearance => appearance;
    public IReadonlyBinding<Instant?> Expiry => expiry;

    public ObservableStatus(Status status)
    {
        this.status = status;

        appearance = Binding.Create(status.Appearance);
        expiry = Binding.Create(status.Expiry);

        status.AppearanceChanged += onAppearanceChanged;
        status.ExpiryChanged += onExpiryChanged;
    }

    public void Dispose()
    {
        status.AppearanceChanged -= onAppearanceChanged;
        status.ExpiryChanged -= onExpiryChanged;
    }

    private void onAppearanceChanged()
    {
        appearance.SetFromSource(status.Appearance);
    }

    private void onExpiryChanged()
    {
        expiry.SetFromSource(status.Expiry);
    }

    public bool Observes(Status other) => status == other;
}
