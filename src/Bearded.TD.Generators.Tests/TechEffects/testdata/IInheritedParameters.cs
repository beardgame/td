using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using JetBrains.Annotations;

namespace Bearded.TD.Generators.Tests.TechEffects;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
interface IInheritedParameters  : IParametersTemplate<IInheritedParameters>,  IBaseInterface
{
    int LocalParameter { get; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
interface IBaseInterface
{
    double BaseParameter { get; }
}
