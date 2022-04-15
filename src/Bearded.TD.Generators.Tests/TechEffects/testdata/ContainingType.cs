using Bearded.TD.Shared.TechEffects;
using JetBrains.Annotations;

namespace Bearded.TD.Generators.Tests.TechEffects;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class ContainingType
{
    public interface INestedParameters : IParametersTemplate<INestedParameters>
    {
        int MyInt { get; }
    }
}
