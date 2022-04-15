using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Generators.Tests.TechEffects
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    interface IMyParameters : IParametersTemplate<IMyParameters>
    {
        int RawType { get; }
        int? NullableType { get; }
        Unit WrappedType { get; }

        [Modifiable(0.5)]
        double RawTypeWithDefault { get; }
        [Modifiable(true)]
        bool BoolTypeWithDefault { get; }

        [Modifiable(10, Type = AttributeType.Damage)]
        int ModifiableRawType { get; }
        [Modifiable(5, Type = AttributeType.Range)]
        Unit ModifiableWrappedType { get; }

        [ConvertsAttribute]
        static AttributeConverter<Unit> UnitConverter =
            new(d => new Unit((float) d), u => u.NumericValue);
    }
}
