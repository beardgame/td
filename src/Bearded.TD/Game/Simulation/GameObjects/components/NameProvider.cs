using Bearded.TD.Content.Models;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("name")]
sealed class NameProvider : Component<IName>, INameProvider
{
    public string Name => Parameters.Name;

    public NameProvider(IName parameters) : base(parameters) { }
    protected override void OnAdded() {}
    public override void Update(TimeSpan elapsedTime) {}
}

interface INameProvider
{
    string Name { get; }
}

static class NameProviderExtensions
{
    public static string NameOrDefault(this INameProvider? nameProvider) => nameProvider?.Name ?? "Structure";
}
