using System.Collections.Immutable;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("chooseOneOf")]
sealed class ChooseOneOf : Component<ChooseOneOf.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ImmutableArray<IComponentFactory> Components { get; }
    }

    public ChooseOneOf(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        if (Parameters.Components.IsDefaultOrEmpty)
        {
            Owner.RemoveComponent(this);
            return;
        }

        var chosenComponent = Parameters.Components.RandomElement();
        Owner.AddComponent(chosenComponent.Create());
        Owner.RemoveComponent(this);
    }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime) { }
}

