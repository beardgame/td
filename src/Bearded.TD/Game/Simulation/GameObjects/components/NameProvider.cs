using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("name")]
sealed class NameProvider : Component<NameProvider.IParameters>, INameProvider
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        public string Name { get; }
    }

    public string Name => Parameters.Name;

    public NameProvider(IParameters parameters) : base(parameters) { }
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
